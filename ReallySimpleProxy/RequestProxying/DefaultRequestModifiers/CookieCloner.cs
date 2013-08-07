using System;
using System.Net;
using Nancy;

namespace ReallySimpleProxy.RequestProxying.DefaultRequestModifiers
{
    public class CookieCloner : IRequestModifier
    {
        public void Modify(string outgoingUri, Request incomingRequest, HttpWebRequest outgoingRequest)
        {
            outgoingRequest.CookieContainer = new CookieContainer();
            foreach (var cookie in incomingRequest.Cookies)
            {
                outgoingRequest.CookieContainer.Add(new Uri(outgoingUri), new Cookie(cookie.Key, cookie.Value));
            }
        }
    }
}
