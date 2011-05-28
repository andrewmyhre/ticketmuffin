using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GroupGiving.Core.Data.Azure;
using GroupGiving.Web.Code;
using Ninject;

namespace GroupGiving.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : Ninject.Web.NinjectHttpApplication
    {
        public static IKernel Kernel { get; set; }
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        /*
        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }*/

        protected override void OnApplicationStarted()
        {
            log4net.Config.XmlConfigurator.Configure();
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            new EntityMappings().CreateMaps();
            base.OnApplicationStarted();
        }

        protected override IKernel CreateKernel()
        {
            Kernel = new StandardKernel(new GroupGivingNinjectModule());
            return Kernel;
        }
    }
}