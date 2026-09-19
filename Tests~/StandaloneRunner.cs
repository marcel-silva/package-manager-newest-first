using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

// Minimal runner for these synchronous NUnit tests when the Editor test runner is unavailable.
// Does not pretend to implement NUnit lifecycle/UnityTest support.
internal static class StandaloneRunner
{
    private static int Main()
    {
        int passed = 0, failed = 0;
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "NewestFirst.Tests"))
        foreach (var method in type.GetMethods())
        {
            var cases = method.GetCustomAttributes(typeof(TestCaseAttribute), false).Cast<TestCaseAttribute>().ToArray();
            var arguments = cases.Select(c => c.Arguments).ToArray();
            if (arguments.Length == 0 && method.IsDefined(typeof(TestAttribute), false)) arguments = new[] { new object[0] };
            foreach (var args in arguments)
            {
                try
                {
                    method.Invoke(Activator.CreateInstance(type), args);
                    Console.WriteLine("PASS " + method.Name + " " + string.Join(",", args));
                    passed++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("FAIL " + method.Name + ": " + (ex.InnerException ?? ex));
                    failed++;
                }
            }
        }
        Console.WriteLine("Passed: " + passed + "; Failed: " + failed);
        return failed == 0 && passed > 0 ? 0 : 1;
    }
}
