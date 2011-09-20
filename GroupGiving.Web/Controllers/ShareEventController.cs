﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EmailProcessing;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;
using GroupGiving.Web.Models;
using Ninject;

namespace GroupGiving.Web.Controllers
{
    public class ShareEventController : Controller
    {
        private readonly IRepository<GroupGivingEvent> _eventRepository;
        private readonly IEmailPackageRelayer _emailRelayer;

        public ShareEventController(IRepository<GroupGivingEvent> eventRepository, IEmailPackageRelayer emailRelayer)
        {
            _eventRepository = eventRepository;
            _emailRelayer = emailRelayer;
        }

        //
        // GET: /ShareEvent/
        [HttpGet]
        public ActionResult Index(string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(shortUrl))
                return new HttpNotFoundResult();

            var viewModel = new ShareEventViewModel();
            viewModel.Event = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);
            viewModel.ShareUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority,
                                               viewModel.Event.ShortUrl);

            viewModel.ShareViaEmail = new ShareViaEmailViewModel();
            viewModel.ShareViaEmail.ShortUrl = shortUrl;
            viewModel.ShareViaEmail.ShareUrl = viewModel.ShareUrl;

            // todo: store this content in localised templates
            viewModel.ShareViaEmail.Subject = "Come to my event";
            viewModel.ShareViaEmail.Body = string.Format(@"Hi,
I'm organising an event and I'd like you to come. You can read about it and sign up here:

{0}

Thanks!", viewModel.ShareUrl);

            object emailSent = false;
            if (TempData.TryGetValue("email_sent", out emailSent))
            {
                viewModel.EmailSentSuccessfully = (bool)emailSent;
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(string shortUrl, ShareViaEmailViewModel request)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = new ShareEventViewModel();
                viewModel.ShareViaEmail = request;

                viewModel.Event = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);
                viewModel.ShareUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority,
                                                   viewModel.Event.ShortUrl);
                return View("Index", viewModel);

            }

            EmailPackage package = new EmailPackage();
            package.Subject = request.Subject;
            package.Text = request.Body;
            package.From = "noreply@ticketmuffin.com";

            string[] recipientList = request.Recipients.Split(',');
            foreach(var recipient in recipientList)
            {
                package.To.Add(recipient.Trim());
                
            }
            _emailRelayer.Relay(package);

            TempData["email_sent"] = true;
            return RedirectToAction("Index", new { shortUrl });
        }
    }
}
