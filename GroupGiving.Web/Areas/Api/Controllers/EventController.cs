using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GroupGiving.Web.Areas.Api.Controllers
{
    public class EventController : ApiControllerBase
    {
        //
        // GET: /Api/Event/

        public ActionResult Index(string shortUrl)
        {
            var @event = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);

            if (Request.AcceptTypes.Contains("application/json"))
            {
                return Json(@event);
            } 

            return Xml(@event);
        }

    }
}
