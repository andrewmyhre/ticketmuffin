using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Domain;
using Raven.Client;

namespace GroupGiving.Web.Areas.Api.Controllers
{
    [ValidateInput(false)]
    public class ContentController : Controller
    {
        private readonly IDocumentSession _session;

        public ContentController(IDocumentSession session)
        {
            _session = session;
        }

        //
        // GET: /Api/Content/
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Put)]
        public ActionResult CreateOrUpdate(int pageId, string contentLabel, string culture, string contentValue)
        {
            HttpStatusCodeResult result = new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            var page = _session.Load<PageContent>(pageId);
            if (page == null)
            {
                return HttpNotFound();
            }
            var contentDefinition = page.Content.SingleOrDefault(c => c.Label == contentLabel);
            if (contentDefinition == null)
            {
                return HttpNotFound();
            }

            var localContent = contentDefinition.ContentByCulture.SingleOrDefault(lc => lc.Culture == culture);
            if (localContent != null)
            {
                localContent.Value = contentValue;
                result = new HttpStatusCodeResult((int)HttpStatusCode.Accepted);
            } else
            {
                contentDefinition.ContentByCulture.Add(new LocalisedContent(){Culture = culture, Value = contentValue});
                result = new HttpStatusCodeResult((int)HttpStatusCode.Created);
            }
            _session.SaveChanges();

            
            return result;
        }

    }
}
