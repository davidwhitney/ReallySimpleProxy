using System.Net;
using Nancy;
using NUnit.Framework;
using ReallySimpleProxy.RequestProxying.DefaultRequestModifiers;

namespace ReallySimpleProxy.Test.Unit.RequestProxying.DefaultRequestModifiers
{
    [TestFixture]
    public class CookieClonerTests
    {
        private Url _url;
        private CookieCloner _mod;
        private HttpWebRequest _outgoingRequest;

        [SetUp]
        public void SetUp()
        {
            _url = new Url { HostName = "tempuri.org", Path = "/", Port = 80, Scheme = "http" };
            _mod = new CookieCloner();
            _outgoingRequest = (HttpWebRequest)WebRequest.Create(_url.ToString());
        }

        [Test]
        public void CloneRequestForForwarding_CookiesExistInOriginRequest_CookiesCopiedToProxiedRequest()
        {
            var req = new Request("POST", _url);
            req.Cookies.Add("cookie", "value");

            _mod.Modify(_url.ToString(), req, _outgoingRequest);

            Assert.That(_outgoingRequest.CookieContainer.Count, Is.EqualTo(1));
            Assert.That(_outgoingRequest.CookieContainer.GetCookies(_url)["cookie"].Value, Is.EqualTo("value"));
        }
    }
}
