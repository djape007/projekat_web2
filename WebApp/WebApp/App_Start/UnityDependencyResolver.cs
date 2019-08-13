using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Http.Dependencies;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using WebApp.Models;
using WebApp.Persistence;
using WebApp.Persistence.Repository;
using WebApp.Persistence.UnitOfWork;
using WebApp.Providers;

namespace WebApp.App_Start
{
    public class UnityResolver : IDependencyResolver
    {
        protected IUnityContainer container;

        public UnityResolver(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            this.container = container;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return container.Resolve(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return container.ResolveAll(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return new List<object>();
            }
        }

        public IDependencyScope BeginScope()
        {
            var child = container.CreateChildContainer();
            return new UnityResolver(child);
        }

        public void RegisterTypes()
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // TODO: Register your type's mappings here.
            // container.RegisterType<IProductRepository, ProductRepository>();
           
            container.RegisterType<DbContext, ApplicationDbContext>(new PerResolveLifetimeManager());
            container.RegisterType<IUnitOfWork, DemoUnitOfWork>();
            container.RegisterType<IBusRepository, BusRepository>();
            container.RegisterType<ILineRepository, LineRepository>();
            container.RegisterType<IPriceHistoryRepository, PriceHistoryRepository>();
            container.RegisterType<IPricelistRepository, PricelistRepository>();
            container.RegisterType<IProductTypeRepository, ProductTypeRepository>();
            container.RegisterType<ITimeTableRepository, TimeTableRepository>();
            container.RegisterType<IBusStopRepository, BusStopRepository>();
            container.RegisterType<IBusStopsOnLineRepository, BusStopsOnLineRepository>();
            container.RegisterType<ICoefficientRepository, CoefficientRepository>();
            container.RegisterType<IPointPathLineRepository, PointPathLineRepository>();
            container.RegisterType<ISoldTicketRepository, SoldTicketRepository>();
            container.RegisterType<IUserRepository, UserRepository>();

            //container.RegisterType<IServiceRepository, ServiceRepository>();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            container.Dispose();
        }
    }
}