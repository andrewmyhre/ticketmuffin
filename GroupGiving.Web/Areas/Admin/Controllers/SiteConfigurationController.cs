﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Domain;
using GroupGiving.PayPal.Configuration;
using Raven.Client;

namespace GroupGiving.Web.Areas.Admin.Controllers
{
    public class SiteConfigurationController : Controller
    {
        private readonly IDocumentStore _documentStore;

        public SiteConfigurationController(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        //
        // GET: /Admin/SiteConfiguration/

        public ActionResult Index()
        {
            using (var session = _documentStore.OpenSession())
            {
                var configuration = session.Query<SiteConfiguration>().FirstOrDefault();
                if (configuration == null)
                {
                    configuration = new SiteConfiguration();
                    configuration.AdaptiveAccountsConfiguration = new AdaptiveAccountsConfiguration();
                    configuration.DatabaseConfiguration = new DatabaseConfiguration();
                    configuration.JustGivingApiConfiguration = new JustGivingApiConfiguration();
                }

                return View(configuration);
            }
        }

        [HttpPost]
        public ActionResult Index(SiteConfiguration model)
        {
            using (var session = _documentStore.OpenSession())
            {
                bool newConfiguration = false;
                var configuration = session.Query<SiteConfiguration>().FirstOrDefault();
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
                    configuration.AdaptiveAccountsConfiguration.SandboxApiBaseUrl =
                        configuration.AdaptiveAccountsConfiguration.SandboxApiBaseUrl.TrimEnd('/');
                    configuration.AdaptiveAccountsConfiguration.LiveApiBaseUrl
                        = configuration.AdaptiveAccountsConfiguration.LiveApiBaseUrl.TrimEnd('/');

                    if (newConfiguration)
                    {
                        session.Store(configuration);
                    }
                    session.SaveChanges();
                } else
                {
                    return View(model);
                }
            }

            return RedirectToAction("Index");
        }

    }
}
