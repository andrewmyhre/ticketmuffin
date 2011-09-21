﻿using System;
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

    public class MvcApplication : System.Web.HttpApplication
    {
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

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

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
                new {controller = "Account", action = "ResetPassword"},
                new [] {"GroupGiving.Web.Controllers"});

            routes.MapRoute(
                "SignUp",
                "signup",
                new { controller = "Account", action = "signup" },
                new[] { "GroupGiving.Web.Controllers" });
            routes.MapRoute(
                "SignIn",
                "signin",
                new { controller = "Account", action = "signin" },
                new[] { "GroupGiving.Web.Controllers" });
        }

        private static void MapEventRoutes(RouteCollection routes)
        {
            routes.MapRoute("Event_Details",
                            "events/{shortUrl}",
                            new {controller = "Event", action = "index"},
                            new[]{"GroupGiving.Web.Controllers"});

            routes.MapRoute(
                "Event_ShareYourEvent",
                "events/{shortUrl}/share",
                new {controller = "ShareEvent", action = "Index"});
            routes.MapRoute(
                "Event_Pledge",
                "events/{shortUrl}/pledge",
                new { controller = "Event", action = "pledge" },
                            new[] { "GroupGiving.Web.Controllers" });
            routes.MapRoute(
                "Event_Edit",
                "events/{shortUrl}/edit",
                new { controller = "Event", action = "edit-event" },
                            new[] { "GroupGiving.Web.Controllers" });
            routes.MapRoute(
                "Event_ListPledges",
                "events/{shortUrl}/pledges",
                new { controller = "Event", action = "event-pledges" },
                            new[] { "GroupGiving.Web.Controllers" });

            routes.MapRoute(
                "Pledge_Refund",
                "events/{shortUrl}/pledges/{orderNumber}/refund",
                new { controller = "Event", action = "refund-pledge" },
                            new[] { "GroupGiving.Web.Controllers" });

            routes.MapRoute(
                "Events",
                "events/{shortUrl}/{action}",
                new { controller = "Event", action = "index" },
                            new[] { "GroupGiving.Web.Controllers" });
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

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            PageContent.Initialise(System.Web.Hosting.HostingEnvironment.MapPath("~/content/PageContent"));

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.AddIPhone<RazorViewEngine>();
            ViewEngines.Engines.AddGenericMobile<RazorViewEngine>();
            ViewEngines.Engines.Add(new RazorViewEngine());

            try
            {
                EmailFacade = EmailFacadeFactory.CreateFromConfiguration();
                EmailFacade.LoadTemplates();
            }
            catch (Exception e)
            {
                _logger.Fatal("Failed to load email sender", e);
            }
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