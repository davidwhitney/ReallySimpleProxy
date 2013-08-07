using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceProcess;
using Nancy.Hosting.Self;
using SimpleServices;

namespace ReallySimpleProxy
{
    [RunInstaller(true)]
    public class ReallySimpleProxyHost
    {
        public void SelfHost(string[] args, string serviceName, string host, int port)
        {
            var hostClass = new HttpHost(host, port);

            new Service(args,
                new List<IWindowsService> { hostClass }.ToArray,
                installationSettings: (serviceInstaller, serviceProcessInstaller) =>
                {
                    serviceInstaller.ServiceName = serviceName;
                    serviceInstaller.StartType = ServiceStartMode.Manual;
                    serviceProcessInstaller.Account = ServiceAccount.LocalService;
                },
                configureContext: x => { x.Log = Console.WriteLine; })
                .Host();
        }

        private class HttpHost : IWindowsService
        {
            public ApplicationContext AppContext { get; set; }
            private readonly NancyHost _nancyHost;

            public HttpHost(string host, int port)
            {
                _nancyHost = new NancyHost(new Uri("http://" + host + ":" + port));
            }

            public void Start(string[] args)
            {
                AppContext.Log("Starting ReallySimpleProxy.");
                _nancyHost.Start();
            }

            public void Stop()
            {
                AppContext.Log("Shutdown ReallySimpleProxy.");
                _nancyHost.Stop();
            }
        }
    }
}
