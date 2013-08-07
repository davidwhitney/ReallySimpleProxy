using System;

namespace ReallySimpleProxy.RequestProxying
{
    public interface IRequestModifier
    {
        void Modify(string outgoingUri, Nancy.Request incomingRequest, System.Net.HttpWebRequest outgoingRequest);
    }
}