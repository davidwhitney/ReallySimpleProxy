using Nancy;

namespace ReallySimpleProxy.RequestProxying
{
    public interface IProxy
    {
        dynamic ProxyRequest(NancyContext ctx);
    }
}