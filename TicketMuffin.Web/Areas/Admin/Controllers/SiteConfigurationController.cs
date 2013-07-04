﻿using System.Linq;
using System.Web.Mvc;
using Raven.Client;
using TicketMuffin.Core.Configuration;
using TicketMuffin.PayPal.Configuration;
using TicketMuffin.Web.Configuration;

namespace TicketMuffin.Web.Areas.Admin.Controllers
{
    public class SiteConfigurationController : Controller
    {
        private readonly IDocumentSession _documentSession;

        public SiteConfigurationController(IDocumentSession documentSession)
        {
            _documentSession = documentSession;
        }

        //
        // GET: /Admin/SiteConfiguration/

        public ActionResult Index()
        {
            var configuration = _documentSession.Query<SiteConfiguration>().FirstOrDefault();
            if (configuration == null)
            {
                configuration = new SiteConfiguration();
                configuration.AdaptiveAccountsConfiguration = new AdaptiveAccountsConfiguration();
                configuration.DatabaseConfiguration = new DatabaseConfiguration();
                configuration.JustGivingApiConfiguration = new JustGivingApiConfiguration();
            }

            return View(configuration);
        }

        [HttpPost]
        public ActionResult Index(SiteConfiguration model)
        {
            bool newConfiguration = false;
            var configuration = _documentSession.Query<SiteConfiguration>().SingleOrDefault();
            if (configuration == null)
            {
                newConfiguration = true;
                configuration = new SiteConfiguration();
                configuration.AdaptiveAccountsConfiguration = new AdaptiveAccountsConfiguration();
                configuration.DatabaseConfiguration = new DatabaseConfiguration();
                configuration.JustGivingApiConfiguration = new JustGivingApiConfiguration();
            }

            if (TryUpdateModel(configuration, "", null, new[] {"Id"}))
            {
                configuration.AdaptiveAccountsConfiguration.ApiBaseUrl
                    = configuration.AdaptiveAccountsConfiguration.ApiBaseUrl.TrimEnd('/');

                if (newConfiguration)
                {
                    _documentSession.Store(configuration);
                }
                _documentSession.SaveChanges();
            } else
            {
                return View(model);
            }

            return RedirectToAction("Index");
        }

    }
}
