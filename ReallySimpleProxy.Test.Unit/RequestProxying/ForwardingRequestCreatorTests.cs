using System;
using System.Collections.Generic;
using System.IO;
using Nancy;
using Nancy.IO;
using NUnit.Framework;
using ReallySimpleProxy.RequestProxying;

namespace ReallySimpleProxy.Test.Unit.RequestProxying
{
    [TestFixture]
    public class ForwardingRequestCreatorTests
    {
        private ForwardingRequestCreator _creator;
        private List<IRequestModifier> _requestModifieres;
        private List<IRequestBodyProcessor> _bodyModifieres;
        private Url _url;

        [SetUp]
        public void SetUp()
        {
            _url = new Url { HostName = "tempuri.org", Path = "/", Port = 80, Scheme = "http" };
            _requestModifieres = new List<IRequestModifier>();
            _bodyModifieres = new List<IRequestBodyProcessor>();
            _creator = new ForwardingRequestCreator(_requestModifieres, _bodyModifieres);
        }

        [TestCase("GET")]
        [TestCase("HEAD")]
        [TestCase("DELETE")]
        public void ProcessBodyHandlers_ForVerbsWhereBodiesArentSupported_ReturnsNull(string verb)
        {
            var stream = GenerateStreamFromString("bodytest");
            var req = new Request(verb, _url, new RequestStream(stream, stream.Length, false));
            
            var supportsBody = _creator.ProcessBodyHandlers(req);

            Assert.That(supportsBody, Is.Null);
        }

        [TestCase("POST")]
        [TestCase("PUT")]
        public void ProcessBodyHandlers_ForVerbsWhereBodiesAreSupported_ReturnsBody(string verb)
        {
            var stream = GenerateStreamFromString("bodytest");
            var req = new Request(verb,_url, new RequestStream(stream, stream.Length, false));

            var supportsBody = _creator.ProcessBodyHandlers(req);

            Assert.That(supportsBody, Is.EqualTo("bodytest"));
        }

        [Test]
        public void BuildOutgoingUrl_RemovesPortFromIncomingRequest()
        {
            var req = new Request("POST", _url);

            var url = _creator.BuildOutgoingUrl(req);

            Assert.That(url, Is.EqualTo("http://tempuri.org"));
        }

        [Test]
        public void BuildOutgoingUrl_WhenXForwardedForProtoHeaderExists_ConvertsSchemeToProtocolInHeader()
        {
            var req = new Request("POST", _url, headers: new Dictionary<string, IEnumerable<string>> {{"X-Forwarded-Proto", new[] {"https"}}});

            var url = _creator.BuildOutgoingUrl(req);

            Assert.That(url, Is.EqualTo("https://tempuri.org"));
        }

        [Test]
        public void CloneRequest_RequestSupplied_CreatesForwardRequest()
        {
            var req = new Request("POST", _url);

            var outgoingReq = _creator.CloneRequest(req);

            Assert.That(outgoingReq, Is.Not.Null);
        }

        [Test]
        public void CloneRequestForForwarding_IncomingRequestIsNull_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => _creator.CloneRequest(null));

            Assert.That(ex.ParamName, Is.EqualTo("incomingRequest"));
        }

        [Test]
        public void ProcessBodyHandlers_BodyModifierExists_ModifierExecuted()
        {
            var modifier = new FakeBodyModifier();
            _bodyModifieres.Add(modifier);

            var stream = GenerateStreamFromString("bodytest");
            var req = new Request("POST", _url, new RequestStream(stream, stream.Length, false));

            _creator.ProcessBodyHandlers(req);

            Assert.That(modifier.Called, Is.True);
        }

        private class FakeBodyModifier : IRequestBodyProcessor
        {
            public bool Called { get; private set; }
            public string ProcessBody(string currentBody)
            {
                Called = true;
                return currentBody;
            }
        }

        public Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
