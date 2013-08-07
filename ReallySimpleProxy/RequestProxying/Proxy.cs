using System.IO;
using System.Net;
using Nancy;

namespace ReallySimpleProxy.RequestProxying
{
    public class Proxy : IProxy
    {
        private readonly IForwardingRequestCreator _requestCreator;

        public Proxy(IForwardingRequestCreator requestCreator)
        {
            _requestCreator = requestCreator;
        }

        public dynamic ProxyRequest(NancyContext ctx)
        {
            var request = _requestCreator.CloneRequest(ctx.Request);
            var body = _requestCreator.ProcessBodyHandlers(ctx.Request);
            
            if (body != null)
            {
                var requestStream = request.GetRequestStream();
                using (var writer = new StreamWriter(requestStream))
                {
                    writer.Write(body);
                    writer.Flush();
                }
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            return new Nancy.Responses.StreamResponse(() => responseStream, response.ContentType);
        }
    }
}