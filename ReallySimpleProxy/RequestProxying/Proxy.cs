using System.IO;
using System.Net;
using log4net;
using Nancy;

namespace ReallySimpleProxy.RequestProxying
{
    public class Proxy : IProxy
    {
        private readonly IForwardingRequestCreator _requestCreator;
        private static readonly ILog Log = LogManager.GetLogger("Log");

        public Proxy(IForwardingRequestCreator requestCreator)
        {
            _requestCreator = requestCreator;
        }

        public dynamic ProxyRequest(NancyContext ctx)
        {
            Log.Debug("Request incoming to " + ctx.Request.Url);

            var request = _requestCreator.CloneRequest(ctx.Request);
            request.Proxy = null;

            var body = _requestCreator.ProcessBodyHandlers(ctx.Request);
            
            if (body != null)
            {
                Log.Debug("Sending body..."); 
                
                var requestStream = request.GetRequestStream();
                using (var writer = new StreamWriter(requestStream))
                {
                    writer.Write(body);
                    writer.Flush();
                }
            }

            Log.Debug("Streaming response...");

            WebResponse response;
            try
            {
                response = request.GetResponse();
            }
            catch (WebException wex)
            {
                response = wex.Response;
            }

            var responseStream = response.GetResponseStream();
            return new Nancy.Responses.StreamResponse(() => responseStream, response.ContentType);
        }
    }
}