using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NewestFirst
{
    public enum WorkaroundMode { Auto, Enabled, Disabled }

    [InitializeOnLoad]
    public static class NewestFirstExtension
    {
        private const string MenuRoot = "Tools/Package Manager Newest First/";
        private const string Prefix = "UnityEditor.PackageManager.UI.Internal.";
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        private static readonly string Preference = "NewestFirst.Mode." + Hash128.Compute(Application.dataPath);
        private static object container, api, originalFactory, proxy;
        private static FieldInfo factoryField;
        private static MethodInfo resolve, request;
        private static Assembly editorAssembly;
        private static bool reverse, probing, failed;
        private static int generation;
        private static double nextCheck, probeDeadline;
        private static DateTimeOffset[] ascDates, descDates;

        public static string Status { get; private set; } = "Waiting for Package Manager services.";
        public static int RewrittenRequests { get; private set; }
        public static bool IsInstalled => proxy != null;
        public static bool IsReversing => IsInstalled && reverse && !probing && Mode != WorkaroundMode.Disabled;
        public static WorkaroundMode Mode
        {
            get => (WorkaroundMode)Mathf.Clamp(EditorPrefs.GetInt(Preference, 0), 0, 2);
            set
            {
                if (!Enum.IsDefined(typeof(WorkaroundMode), value)) throw new ArgumentOutOfRangeException(nameof(value));
                Uninstall();
                EditorPrefs.SetInt(Preference, (int)value);
                failed = false;
                nextCheck = 0;
                Tick();
                if (value == WorkaroundMode.Disabled) TryRefresh();
            }
        }

        static NewestFirstExtension()
        {
            EditorApplication.update += Tick;
            AssemblyReloadEvents.beforeAssemblyReload += Uninstall;
            EditorApplication.quitting += Uninstall;
        }

        private static object Service(string name) => resolve.MakeGenericMethod(editorAssembly.GetType(Prefix + name, true))
            .Invoke(container, null);

        private static void Install()
        {
            // Deliberately narrow until more Editor versions have been tested.
            if (!Application.unityVersion.StartsWith("6000.4.", StringComparison.Ordinal) &&
                Application.unityVersion != "6000.7.0b1")
                throw new NotSupportedException("This test build supports Unity 6000.4.x and 6000.7.0b1 only.");
            editorAssembly = typeof(Editor).Assembly;
            var services = editorAssembly.GetType(Prefix + "ServicesContainer", true);
            container = services.GetProperty("instance", Flags)?.GetValue(null);
            resolve = services.GetMethods(Flags).Single(m => m.Name == "Resolve" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0);
            if (container == null) throw new InvalidOperationException("ServicesContainer is unavailable.");
            api = Service("IAssetStoreRestAPI");
            factoryField = api.GetType().GetField("m_HttpClientFactory", Flags);
            request = api.GetType().GetMethod("HandleHttpRequest", Flags);
            if (factoryField == null || request == null || request.GetParameters().Length != 6)
                throw new NotSupportedException("Asset Store service signatures changed.");
            var interfaceType = editorAssembly.GetType(Prefix + "IHttpClientFactory", true);
            var get = interfaceType.GetMethod("GetASyncHTTPClient");
            if (get == null || get.GetParameters().Length != 1 || get.GetParameters()[0].ParameterType != typeof(string))
                throw new NotSupportedException("HTTP factory signature changed.");
            originalFactory = factoryField.GetValue(api);
            if (originalFactory == null) throw new InvalidOperationException("HTTP factory is unavailable.");
            var decorator = new FactoryProxy(interfaceType, originalFactory, () => IsReversing, () => RewrittenRequests++);
            proxy = decorator.GetTransparentProxy();
            factoryField.SetValue(api, proxy);
        }

        private static void Tick()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (Mode == WorkaroundMode.Disabled) { Status = "Disabled; Unity handles requests unchanged."; return; }
            if (failed) return;
            var now = EditorApplication.timeSinceStartup;
            if (probing)
            {
                if (now > probeDeadline) FinishUnknown("Detection timed out; requests are unchanged.");
                return;
            }
            if (now < nextCheck) return;
            nextCheck = now + 900;
            try
            {
                if (!IsInstalled) Install();
                // A service reload may replace our decorator. Reattach to the new service only.
                var currentApi = Service("IAssetStoreRestAPI");
                if (!ReferenceEquals(currentApi, api) || !ReferenceEquals(factoryField.GetValue(api), proxy))
                {
                    Uninstall();
                    Install();
                }
                if (Mode == WorkaroundMode.Enabled)
                {
                    reverse = true;
                    Status = "Enabled: purchase-date desc requests are sent as asc.";
                    TryRefresh();
                }
                else Probe();
            }
            catch (Exception exception)
            {
                Uninstall();
                failed = true;
                Status = "Unavailable; Unity is unchanged. " + (exception.InnerException ?? exception).GetType().Name;
                Debug.LogWarning("[Newest First] " + Status + " Use Recheck after changing Unity versions or services.");
            }
        }

        private static void Probe()
        {
            probing = true;
            reverse = false;
            ascDates = descDates = null;
            probeDeadline = EditorApplication.timeSinceStartup + 30;
            var token = ++generation;
            Status = "Checking purchase order using two read-only requests (five items each).";
            foreach (var direction in new[] { "asc", "desc" })
            {
                var captured = direction;
                Action<Dictionary<string, object>> callback = data =>
                {
                    if (!probing || generation != token) return;
                    if (captured == "asc") ascDates = SortPolicy.ReadDates(data);
                    else descDates = SortPolicy.ReadDates(data);
                    if (ascDates == null || descDates == null) return;
                    var detected = SortPolicy.Detect(ascDates, descDates);
                    probing = false;
                    reverse = detected == true;
                    Status = detected == true ? "Auto: reversed service order detected; correction active." :
                        detected == false ? "Auto: service order is correct; no correction needed." :
                        "Auto: inconclusive purchase dates; requests are unchanged. Use Enabled to override.";
                    // Data is kept only for the comparison, never logged or written to disk.
                    ascDates = descDates = null;
                    TryRefresh();
                };
                var errorType = request.GetParameters()[2].ParameterType;
                var argument = Expression.Parameter(errorType.GetGenericArguments()[0], "error");
                Action<object> onError = unused =>
                {
                    if (probing && generation == token) FinishUnknown("Detection failed (sign-in/network/service); requests are unchanged.");
                };
                var error = Expression.Lambda(errorType,
                    Expression.Invoke(Expression.Constant(onError), Expression.Convert(argument, typeof(object))), argument).Compile();
                request.Invoke(api, new object[] {
                    "/-/api/purchases?offset=0&limit=5&orderBy=purchased_date&order=" + direction,
                    callback, error, null, "NewestFirst.Probe." + direction, true
                });
            }
        }

        private static void FinishUnknown(string status)
        {
            probing = false;
            reverse = false;
            generation++;
            ascDates = descDates = null;
            Status = status;
            nextCheck = EditorApplication.timeSinceStartup + 900;
            TryRefresh();
        }

        private static void TryRefresh()
        {
            try
            {
                if (container == null || resolve == null) return;
                var manager = Service("IPageManager");
                var page = manager.GetType().GetProperty("activePage", Flags)?.GetValue(manager);
                if (page == null || page.GetType().Name != "MyAssetsPage") return;
                var handler = Service("IPageRefreshHandler");
                var pageType = editorAssembly.GetType(Prefix + "IPage", true);
                handler.GetType().GetMethod("Refresh", Flags, null, new[] { pageType }, null)?.Invoke(handler, new[] { page });
            }
            catch { /* Refresh is optional; the user's next refresh still uses the decorator. */ }
        }

        private static void Uninstall()
        {
            generation++;
            probing = reverse = false;
            ascDates = descDates = null;
            try
            {
                if (proxy != null && api != null && ReferenceEquals(factoryField.GetValue(api), proxy))
                    factoryField.SetValue(api, originalFactory);
            }
            finally { proxy = null; originalFactory = null; }
        }

        [MenuItem(MenuRoot + "Auto (detect service order)")]
        private static void Auto() => Mode = WorkaroundMode.Auto;
        [MenuItem(MenuRoot + "Enabled (force asc)")]
        private static void Enable() => Mode = WorkaroundMode.Enabled;
        [MenuItem(MenuRoot + "Disabled")]
        private static void Disable() => Mode = WorkaroundMode.Disabled;
        [MenuItem(MenuRoot + "Auto (detect service order)", true)]
        private static bool CheckAuto() { Menu.SetChecked(MenuRoot + "Auto (detect service order)", Mode == WorkaroundMode.Auto); return true; }
        [MenuItem(MenuRoot + "Enabled (force asc)", true)]
        private static bool CheckEnabled() { Menu.SetChecked(MenuRoot + "Enabled (force asc)", Mode == WorkaroundMode.Enabled); return true; }
        [MenuItem(MenuRoot + "Disabled", true)]
        private static bool CheckDisabled() { Menu.SetChecked(MenuRoot + "Disabled", Mode == WorkaroundMode.Disabled); return true; }
        [MenuItem(MenuRoot + "Recheck now")]
        public static void Recheck() { failed = false; generation++; probing = false; nextCheck = 0; Tick(); }
        [MenuItem(MenuRoot + "Status")]
        private static void ShowStatus() => EditorUtility.DisplayDialog("Package Manager Newest First",
            Status + "\n\nMode: " + Mode + "\nRequests corrected this session: " + RewrittenRequests +
            "\nUnity: " + Application.unityVersion, "OK");
    }
}
