using System.Web.Mvc;

namespace TicketMuffin.Web.Code
{
    public class CultureViewEngine : System.Web.Mvc.RazorViewEngine
    {
        public CultureViewEngine()
        {
            //base.ViewLocationFormats = base.ViewLocationFormats.Union(new string[] {"~/Views/{1}/{2}/{0}.cshtml"}).ToArray();
            //base.ViewLocationFormats = new string[] { "~/Views/{2}/{1}/{0}.cshtml", "~/Views/{1}/en-GB/{0}.cshtml", "~/Views/{1}/{0}.cshtml" };
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            //var culture = (string)controllerContext.RouteData.Values["culture"] ?? "en";
            var culture = controllerContext.RequestContext.HttpContext.Request.Cookies["culture"] != null ? controllerContext.RequestContext.HttpContext.Request.Cookies["culture"].Value : "en";
            var result = base.FindView(controllerContext, culture + "/" + viewName, masterName, useCache);

            if (result != null && result.View != null)
            {
                return result;
            }
            return base.FindView(controllerContext, viewName, masterName, useCache);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            //var culture = (string) controllerContext.RouteData.Values["culture"] ?? "en";
            var culture = controllerContext.RequestContext.HttpContext.Request.Cookies["culture"] != null ? controllerContext.RequestContext.HttpContext.Request.Cookies["culture"].Value : "en";
            var result = base.FindPartialView(controllerContext, culture + "/" + partialViewName, useCache);

            if (result != null && result.View != null)
            {
                return result;
            }

            return base.FindPartialView(controllerContext, partialViewName, useCache);
        }
    }
}