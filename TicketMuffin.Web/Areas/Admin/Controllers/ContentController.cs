﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Domain;
using Raven.Client;
using Raven.Client.Linq;

namespace GroupGiving.Web.Areas.Admin.Controllers
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
        // GET: /Admin/Content/

        public ActionResult Index()
        {
            var viewModel = new PageListViewModel();
            viewModel.Pages = _session.Query<GroupGiving.Core.Domain.PageContent>();


            return View(viewModel);
        }

        public ActionResult ViewPageContent(int id)
        {
            var viewModel = new PageViewModel();
            viewModel.Page = _session.Load<PageContent>(id);
            return View(viewModel);
        }

        public ActionResult ViewTranslations(int id, string contentLabel)
        {
            var viewModel = new ContentTranslationViewModel();
            viewModel.Page = _session.Load<PageContent>(id);
            viewModel.Content = viewModel.Page.Content.Where(c => c.Label == contentLabel).SingleOrDefault();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateTranslations(int pageId, string contentLabel)
        {
            var page = _session.Load<PageContent>(pageId);
            var content = page.Content.SingleOrDefault(c => c.Label == contentLabel);

            foreach(var localContent in content.ContentByCulture)
            {
                localContent.Value = Request.Form[localContent.Culture] ?? "";
            }

            _session.SaveChanges();

            return RedirectToAction("ViewTranslations", new {id = pageId, contentLabel = contentLabel});
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddTranslation(int pageId, string contentLabel, string cultureKey, string content)
        {
            var page = _session.Load<PageContent>(pageId);
            var pageContent = page.Content.SingleOrDefault(c => c.Label == contentLabel);
            var localContent = pageContent.ContentByCulture.SingleOrDefault(lc => lc.Culture == cultureKey);
            if (localContent != null)
            {
                localContent.Value = content;
            }
            else
            {
                pageContent.ContentByCulture.Add(new LocalisedContent(){Culture = cultureKey, Value = content});
            }
            _session.SaveChanges();

            return RedirectToAction("ViewTranslations", new {id = pageId, contentLabel = contentLabel});
        }

        public ActionResult ClearAll()
        {
            var pages = _session.Query<PageContent>();
            foreach(var page in pages)
            {
                _session.Delete(page);
            }
            _session.SaveChanges();

            return RedirectToAction("Index");
        }
    }

    public class ContentTranslationViewModel    
    {
        public PageContent Page { get; set; }

        public ContentDefinition Content { get; set; }
    }

    public class PageViewModel
    {
        public PageContent Page { get; set; }
    }

    public class PageListViewModel
    {
        public IRavenQueryable<PageContent> Pages { get; set; }
    }
}