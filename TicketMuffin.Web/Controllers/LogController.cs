using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Raven.Client;

namespace TicketMuffin.Web.Controllers
{
    public class LogController : Controller
    {
        private readonly IDocumentSession _session;
        //
        // GET: /Log/
        public LogController(IDocumentSession session)
        {
            _session = session;
        }

        public ActionResult Index()
        {
            var logs = _session.Query<log4net.Raven.Log>().OrderByDescending(l => l.TimeStamp).ToArray();

            string[] messages = logs.Select(l => string.Join(" - ", l.TimeStamp, l.Level, l.Message)).ToArray();
            return new ContentResult()
                {
                    Content = string.Join("\r\n", messages),
                    ContentType = "text/plain"
                };
        }

    }
}
