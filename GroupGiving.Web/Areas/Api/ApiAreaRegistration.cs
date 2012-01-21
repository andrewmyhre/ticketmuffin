using System.Web.Mvc;
using System.Web.Routing;
using GroupGiving.Web.Areas.Api.Code;

namespace GroupGiving.Web.Areas.Api
{
    public class ApiAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Api";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute("Event api methods",
                             "Api/events/{shortUrl}/{action}",
                             new {controller = "Events", action = "index"});

            context.MapRoute("Content management",
                             "Api/content/{pageid}/{contentLabel}/{culture}",
                             new
                                 {
                                     controller = "Content",
                                     Action = "CreateOrUpdate"
                                 },
                             new
                                 {
                                     httpMethod = new HttpMethodConstraint("PUT")
                                 });

            context.MapRoute(
                "Api_default",
                "Api/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );

            AutoMappingConfiguration.Configure();
            ModelBinderProviders.BinderProviders.Add(new XmlModelBinderProvider());
        }
    }
}
