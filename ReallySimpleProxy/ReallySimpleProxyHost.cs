using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceProcess;
using log4net;
using log4net.Config;
using Nancy.Hosting.Self;
using ReallySimpleProxy.RequestProxying;
using SimpleServices;

namespace ReallySimpleProxy
{
    [RunInstaller(true)]
    public class ReallySimpleProxyHost
    {
        private readonly string[] _args;
        private readonly string _serviceName;
        private static readonly ILog Log = LogManager.GetLogger("Log");

        public ReallySimpleProxyHost(string[] args, string serviceName)
        {
            _args = args;
            _serviceName = serviceName;
        }

        public void Host(string host, int port, List<Type> bodyProcessors = null, List<Type> requestModifiers = null)
        {
            XmlConfigurator.Configure();

            ConfigureInterceptors(bodyProcessors, requestModifiers);

            var hostClass = new HttpHost(host, port);

            new Service(_args,
                new List<IWindowsService> { hostClass }.ToArray,
                installationSettings: (serviceInstaller, serviceProcessInstaller) =>
                {
                    serviceInstaller.ServiceName = _serviceName;
                    serviceInstaller.StartType = ServiceStartMode.Manual;
                    serviceProcessInstaller.Account = ServiceAccount.LocalService;
                },
                configureContext: x => { x.Log = Console.WriteLine; })
                .Host();
        }

        private static void ConfigureInterceptors(List<Type> bodyProcessor, List<Type> requestModifiers)
        {
            Bootstrapper.WhileConfguringContainer = (kernel =>
            {
                foreach (var type in bodyProcessor ?? new List<Type>())
                {
                    Log.Debug("Binding IRequestBodyProcessor to " + type.Name);
                    kernel.Bind<IRequestBodyProcessor>().To(type);
                }
                foreach (var type in requestModifiers ?? new List<Type>())
                {
                    Log.Debug("Binding IRequestModifier to " + type.Name);
                    kernel.Bind<IRequestModifier>().To(type);
                }
            });
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
