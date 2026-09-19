using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace NewestFirst
{
    // A transparent decorator; the original factory still creates and owns HTTP clients.
    // It alone handles credentials, callbacks, cancellation and response parsing.
    internal sealed class FactoryProxy : RealProxy
    {
        private readonly object original;
        private readonly Func<bool> reverse;
        private readonly Action rewritten;

        internal FactoryProxy(Type interfaceType, object original, Func<bool> reverse, Action rewritten)
            : base(interfaceType)
        {
            this.original = original;
            this.reverse = reverse;
            this.rewritten = rewritten;
        }

        public override IMessage Invoke(IMessage message)
        {
            var call = (IMethodCallMessage)message;
            var args = (object[])call.Args.Clone();
            try
            {
                if (call.MethodName == "GetASyncHTTPClient" && args.Length == 1 && args[0] is string url)
                {
                    args[0] = SortPolicy.Rewrite(url, reverse());
                    if (!Equals(args[0], url)) rewritten();
                }
                var result = ((MethodInfo)call.MethodBase).Invoke(original, args);
                return new ReturnMessage(result, args, args.Length, call.LogicalCallContext, call);
            }
            catch (TargetInvocationException exception)
            {
                return new ReturnMessage(exception.InnerException ?? exception, call);
            }
            catch (Exception exception)
            {
                return new ReturnMessage(exception, call);
            }
        }
    }
}
