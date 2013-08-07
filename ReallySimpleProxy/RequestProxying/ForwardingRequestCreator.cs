using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Nancy;

namespace ReallySimpleProxy.RequestProxying
{
    public class ForwardingRequestCreator : IForwardingRequestCreator
    {
        private readonly IEnumerable<IRequestModifier> _requestModifiers;
        private readonly IEnumerable<IRequestBodyProcessor> _bodyProcessors;

        public ForwardingRequestCreator(IEnumerable<IRequestModifier> requestModifiers, IEnumerable<IRequestBodyProcessor> bodyProcessors)
        {
            _requestModifiers = requestModifiers;
            _bodyProcessors = bodyProcessors;
        }

        public HttpWebRequest CloneRequest(Request incomingRequest)
        {
            if (incomingRequest == null) throw new ArgumentNullException("incomingRequest");

            var url = BuildOutgoingUrl(incomingRequest);
            var outgoingRequest = (HttpWebRequest)WebRequest.Create(url);
            outgoingRequest.Method = incomingRequest.Method;

            foreach (var mod in _requestModifiers)
            {
                mod.Modify(url, incomingRequest, outgoingRequest);
            }
            
            return outgoingRequest;
        }

        public string BuildOutgoingUrl(Request req)
        {
            var url = req.Url.ToString().Replace(":" + req.Url.Port, String.Empty);

            if (req.Headers.Keys.Contains("X-Forwarded-Proto"))
            {
                url = url.Replace(req.Url.Scheme, req.Headers.Single(x => x.Key == "X-Forwarded-Proto").Value.First());
            }

            return url;
        }

        public string ProcessBodyHandlers(Request incomingRequest)
        {
            if (!new [] { "POST", "PUT" }.Contains(incomingRequest.Method))
            {
                return null;
            }

            using (var sr = new StreamReader(incomingRequest.Body))
            {
                var initialBody = sr.ReadToEnd();
                return _bodyProcessors.Aggregate(initialBody, (c, p) => p.ProcessBody(c));
            }
        }
    }
}