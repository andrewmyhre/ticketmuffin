﻿using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace TicketMuffin.Web.Code
{
    public class MultiCultureMvcRouteHandler : MvcRouteHandler
    {
        protected override IHttpHandler GetHttpHandler(System.Web.Routing.RequestContext requestContext)
        {
            var culture = requestContext.RouteData.Values["culture"].ToString();
            var ci = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = ci;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
            return base.GetHttpHandler(requestContext);
        }
    }

    public class SingleCultureMvcRouteHandler : MvcRouteHandler
    {
        
    }
}
