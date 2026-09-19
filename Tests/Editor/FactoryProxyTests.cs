using System;
using NUnit.Framework;

namespace NewestFirst.Tests
{
    public interface IFakeFactory
    {
        string GetASyncHTTPClient(string url);
        string PostASyncHTTPClient(string url, string body);
        void AbortByTag(string tag);
        string LastTag { get; }
        void Fail();
    }

    public class FakeFactory : IFakeFactory
    {
        public string LastTag { get; private set; }
        public string GetASyncHTTPClient(string url) => url;
        public string PostASyncHTTPClient(string url, string body) => url + "|" + body;
        public void AbortByTag(string tag) => LastTag = tag;
        public void Fail() => throw new InvalidOperationException("original failure");
    }

    public class FactoryProxyTests
    {
        [Test]
        public void DecoratorForwardsGetPostCancellationPropertiesAndOriginalExceptions()
        {
            var original = new FakeFactory();
            var corrected = 0;
            var reverse = true;
            var type = typeof(SortPolicy).Assembly.GetType("NewestFirst.FactoryProxy", true);
            var decorator = Activator.CreateInstance(type,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, new object[] { typeof(IFakeFactory), original, (Func<bool>)(() => reverse),
                    (Action)(() => corrected++) }, null);
            var proxy = (IFakeFactory)type.GetMethod("GetTransparentProxy").Invoke(decorator, null);
            const string url = "https://example.invalid/-/api/purchases?offset=50&orderBy=purchased_date&order=desc";
            Assert.That(proxy.GetASyncHTTPClient(url), Does.Contain("order=asc"));
            Assert.That(corrected, Is.EqualTo(1));
            Assert.That(proxy.PostASyncHTTPClient(url, "body"), Is.EqualTo(url + "|body"));
            proxy.AbortByTag("normal-cancellation-tag");
            Assert.That(proxy.LastTag, Is.EqualTo("normal-cancellation-tag"));
            Assert.Throws<InvalidOperationException>(() => proxy.Fail());
            reverse = false;
            Assert.That(proxy.GetASyncHTTPClient(url), Is.EqualTo(url));
            Assert.That(corrected, Is.EqualTo(1));
        }
    }
}
