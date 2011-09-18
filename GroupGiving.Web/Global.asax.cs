using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using EmailProcessing;
using GroupGiving.Web.Code;
using log4net;
using Ninject;
using System.Configuration;
using EmailProcessing.Configuration;

namespace GroupGiving.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : Ninject.Web.NinjectHttpApplication
    {
        public static IKernel Kernel { get; set; }
        public static XmlContentProvider PageContent = new XmlContentProvider();
        public static EmailFacade EmailFacade { get; private set; }
        private static ILog _logger = LogManager.GetLogger(typeof(MvcApplication));

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            MapAccountRoutes(routes);
            MapEventCreationRoutes(routes);
            MapEventRoutes(routes);
            MapPurchaseRoutes(routes);

            MapCultureRoutes(routes);

            routes.MapRoute(
                "CultureDefault", // Route name
                "{culture}/{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional, culture="en" },
                new {culture = new CultureConstraint("en", "pl")}// Parameter defaults
            );
            ((Route)routes["CultureDefault"]).RouteHandler = new MultiCultureMvcRouteHandler();

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            
        }

        private static void MapCultureRoutes(RouteCollection routes)
        {
            List<RouteBase> newRoutes = new List<RouteBase>();
            foreach(Route route in routes)
            {
                if (!(route.RouteHandler is SingleCultureMvcRouteHandler))
                {
                    var newRoute = new Route("{culture}/" + route.Url, new MultiCultureMvcRouteHandler());
                    newRoute.Defaults = route.Defaults;
                    newRoute.Constraints = route.Constraints;

                    if (newRoute.Defaults == null)
                    {
                        newRoute.Defaults = new RouteValueDictionary();
                    }
                    newRoute.Defaults.Add("culture", "en");

                    if (newRoute.Constraints == null)
                    {
                        newRoute.Constraints = new RouteValueDictionary();
                    }
                    newRoute.Constraints.Add("culture", new CultureConstraint("en", "pl"));
                    newRoutes.Add(newRoute);
                }
            }

            int index = 0;
            foreach (var route in newRoutes)
            {
                //routes.Insert(index++, route);
                routes.Add(route);
            }
        }

        private static void MapPurchaseRoutes(RouteCollection routes)
        {
            routes.MapRoute("OrderStep1",
                            "pledge/{eventId}",
                            new {controller = "Order", action = "Index"});
            routes.MapRoute("OrderStep2",
                            "pledge/{eventId}/step2",
                            new { controller = "Order", action = "StartRequest" });
        }

        private static void MapAccountRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "ResetPassword",
                "account/resetpassword/{token}",
                new {controller = "Account", action = "ResetPassword"});

            routes.MapRoute(
                "SignUpOrSignIn",
                "signin",
                new { controller = "Account", action = "signup-or-signin" },
                new { httpMethod = new HttpMethodConstraint("GET") });
            routes.MapRoute(
                "SignUp",
                "signup",
                new { controller = "Account", action = "signup" },
                new {httpMethod = new HttpMethodConstraint("POST")});
            routes.MapRoute(
                "SignIn",
                "signin",
                new { controller = "Account", action = "signin" },
                new { httpMethod = new HttpMethodConstraint("POST") });
        }

        private static void MapEventRoutes(RouteCollection routes)
        {
            routes.MapRoute("Event_Details",
                            "events/{shortUrl}",
                            new {controller = "Event", action = "index"});

            routes.MapRoute(
                "Event_ShareYourEvent",
                "events/{shortUrl}/share",
                new {controller = "Event", action = "share"});
            routes.MapRoute(
                "Event_Pledge",
                "events/{shortUrl}/pledge",
                new { controller = "Event", action = "pledge" });
            routes.MapRoute(
                "Event_Edit",
                "events/{shortUrl}/edit",
                new { controller = "Event", action = "edit-event" });
            routes.MapRoute(
                "Event_ListPledges",
                "events/{shortUrl}/pledges",
                new { controller = "Event", action = "event-pledges" });

            routes.MapRoute(
                "Pledge_Refund",
                "events/{shortUrl}/pledges/{orderNumber}/refund",
                new {controller = "Event", action = "refund-pledge"});
        }

        private static void MapEventCreationRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "CreateEvent_EventDetails",
                "events/create",
                new { controller = "CreateEvent", action = "create" });
            routes.MapRoute(
                "CreateEvent_TicketDetails",
                "events/create/{eventId}/tickets",
                new { controller = "CreateEvent", action = "tickets" });

        }

        protected override void OnApplicationStarted()
        {
            log4net.Config.XmlConfigurator.Configure();
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            PageContent.Initialise(System.Web.Hosting.HostingEnvironment.MapPath("~/content/PageContent"));

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.AddIPhone<CultureViewEngine>();
            ViewEngines.Engines.AddGenericMobile<CultureViewEngine>();
            ViewEngines.Engines.Add(new CultureViewEngine());

            try
            {
                EmailFacade = EmailFacadeFactory.CreateFromConfiguration();
                EmailFacade.LoadTemplates();
            }
            catch (Exception e)
            {
                _logger.Fatal("Failed to load email sender", e);
            }

            base.OnApplicationStarted();
        }

        protected override IKernel CreateKernel()
        {
            Kernel = new StandardKernel(new GroupGivingNinjectModule());
            return Kernel;
        }
    }

    public static class EmailFacadeFactory
    {
        internal static EmailFacade CreateFromConfiguration()
        {
            return new EmailFacade(ConfigurationManager.GetSection("emailBuilder") as EmailBuilderConfigurationSection);
        }
    }

}