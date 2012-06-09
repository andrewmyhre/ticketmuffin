﻿using System.Linq;
using System.Web.Mvc;
using Raven.Client;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Web.Areas.Admin.Controllers
{
    public class CharityManagementController : Controller
    {
        private readonly IDocumentSession _documentSession;

        public CharityManagementController(IDocumentSession documentSession)
        {
            _documentSession = documentSession;
        }

        public ActionResult Index()
        {
            var allCharities = _documentSession.Query<Charity>()
                .OrderBy(c => c.Name)
                .Take(1024)
                .ToList();

            return View(allCharities);
        }
    }
}