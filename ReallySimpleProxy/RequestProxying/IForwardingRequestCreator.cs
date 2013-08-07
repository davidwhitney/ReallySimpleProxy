using System.Net;
using Nancy;

namespace ReallySimpleProxy.RequestProxying
{
    public interface IForwardingRequestCreator
    {
        HttpWebRequest CloneRequest(Request incomingRequest);
        string ProcessBodyHandlers(Request incomingRequest);
        string BuildOutgoingUrl(Request req);
    }
}