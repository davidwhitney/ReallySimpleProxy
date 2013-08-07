using System.Linq;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Ninject;
using Ninject.Extensions.Conventions;
using ReallySimpleProxy.RequestProxying;

namespace ReallySimpleProxy
{
    public class Bootstrapper : NinjectNancyBootstrapper
    {
        protected override void ApplicationStartup(IKernel container, IPipelines pipelines)
        {
            pipelines.BeforeRequest += ctx => container.Get<IProxy>().ProxyRequest(ctx);
            
            base.ApplicationStartup(container, pipelines);
        }

        protected override void ConfigureApplicationContainer(IKernel existingContainer)
        {
            existingContainer.Bind(x => x.FromThisAssembly().SelectAllTypes().BindDefaultInterface());
            
            existingContainer.Bind(x => x.FromThisAssembly().SelectAllTypes()
                .Where(y => y.GetInterfaces().Contains(typeof (IRequestBodyProcessor)))
                .BindAllInterfaces());
            
            existingContainer.Bind(x => x.FromThisAssembly().SelectAllTypes()
                .Where(y => y.GetInterfaces().Contains(typeof (IRequestModifier)))
                .BindAllInterfaces());

            base.ConfigureApplicationContainer(existingContainer);
        }
    }
}