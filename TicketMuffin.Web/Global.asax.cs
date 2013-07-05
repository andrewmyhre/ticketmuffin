using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EmailProcessing;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Web.Mvc.Resources;
using TicketMuffin.Core.Services;
using TicketMuffin.Web.App_Start;
using TicketMuffin.Web.Code;
using TicketMuffin.Web.Models;
using log4net;
using Ninject;

namespace TicketMuffin.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static IEmailFacade EmailFacade { get; private set; }
        private static ILog _logger = LogManager.GetLogger(typeof(MvcApplication));
        public static IEnumerable<IWindowsService> Services = null;

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
                "ContentManagement-Update",
                "contentmanager/pagecontents/{pageId}/{contentLabel}/{culture}",
                new {controller = "ContentManager", action = "UpdateContentDefinition"});
            routes.MapRoute(
                "ContentManagement-Add",
                "contentmanager/pagecontents/{pageId}/{contentLabel}",
                new { controller = "ContentManager", action = "UpdateContentDefinition" });

            //MapCultureRoutes(routes);

            routes.MapRoute("Ticket",
                            "ticket/{id}",
                            new {controller = "Ticket", action = "Index"});

            /*routes.MapRoute(
                "CultureDefault", // Route name
                "{culture}/{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional, culture="en" },
                new {culture = new CultureConstraint("en", "pl")}// Parameter defaults
            );*/
            //((Route)routes["CultureDefault"]).RouteHandler = new MultiCultureMvcRouteHandler();

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
                "events/create/{shortUrl}/tickets",
                new { controller = "CreateEvent", action = "tickets" });

        }

        protected void Application_Start()
        {

            log4net.Config.XmlConfigurator.Configure();
            _logger.Info("");
            _logger.Info("");
            _logger.Info("******************************");
            _logger.Info("TicketMuffin Application Start");
            _logger.Info("******************************");

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            //PageContent.Initialise(System.Web.Hosting.HostingEnvironment.MapPath("~/content/PageContent"));

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.AddIPhone<CultureViewEngine>();
            ViewEngines.Engines.AddGenericMobile<CultureViewEngine>();
            ViewEngines.Engines.Add(new CultureViewEngine());

            ModelBinders.Binders.DefaultBinder = new ResourceModelBinder();

            EmailFacade = Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<IEmailFacade>();

            Services = ServiceLocator.Current.GetAllInstances<IWindowsService>();
            foreach(var service in Services)
            {
                service.Start();
            }

            ModelBinders.Binders[typeof(CreateEventRequest)] =new NonEnglishNumbersModelBinder();
            ModelBinders.Binders[typeof(SetTicketDetailsRequest)] = new NonEnglishNumbersModelBinder();
            ModelBinders.Binders[typeof(UpdateEventViewModel)] = new NonEnglishNumbersModelBinder();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var cultureService = ServiceLocator.Current.GetInstance<ICultureService>();
            var context = new HttpContextWrapper(HttpContext.Current);
            if (!cultureService.HasCulture(context))
            {
                cultureService.SetCurrentCulture(context, cultureService.DeterminePreferredCulture(context.Request.UserLanguages));
            }

        }
    }
}