using System;
using System.Web.Mvc;
using System.Web.Routing;
using GroupGiving.Web.Code;
using Ninject;

namespace GroupGiving.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : Ninject.Web.NinjectHttpApplication
    {
        public static IKernel Kernel { get; set; }
        public static XmlContentProvider PageContent = new XmlContentProvider();

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

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


            MapEventCreationRoutes(routes);
            MapEventRoutes(routes);

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

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
            base.OnApplicationStarted();
        }

        protected override IKernel CreateKernel()
        {
            Kernel = new StandardKernel(new GroupGivingNinjectModule());
            return Kernel;
        }
    }
}