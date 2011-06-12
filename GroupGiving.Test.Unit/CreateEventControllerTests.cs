using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using GroupGiving.Core.Services;
using GroupGiving.Test.Common;
using GroupGiving.Web.Controllers;
using GroupGiving.Web.Models;
using Moq;
using NUnit.Framework;
using RavenDBMembership.Web.Models;

namespace GroupGiving.Test.Unit
{
    [TestFixture]
    public class CreateEventControllerTests
    {
        Mock<IAccountService> accountService = null;
        Mock<IMembershipService> membershipService = null;
        Mock<IFormsAuthenticationService> formsAuthenticationService = null;
        private Mock<IEventService> eventService = null;

        [SetUp]
        public void SetUp()
        {
            accountService = new Mock<IAccountService>();
            membershipService = new Mock<IMembershipService>();
            formsAuthenticationService = new Mock<IFormsAuthenticationService>();
            eventService = new Mock<IEventService>();
        }

        [Test]
        public void ValidCreateEventDetailsProvided_ReturnsRedirectToTicketOptions()
        {
            eventService
                .Setup(m => m.CreateEvent(It.IsAny<CreateEventRequest>()))
                .Returns(new CreateEventResult() {Success = true});

            var controller = new CreateEventController(accountService.Object, membershipService.Object,
                                                       formsAuthenticationService.Object, eventService.Object, null);

            var result = controller.EventDetails(new CreateEventRequest()) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteName, Is.StringMatching("CreateEvent_TicketDetails"));
        }

        [Test]
        public void InvalidCreateEventDetailsProvided_ReturnsViewWithInvalidModelState()
        {
            eventService
                .Setup(m => m.CreateEvent(It.IsAny<CreateEventRequest>()))
                .Returns(new CreateEventResult() { Success = false });

            var controller = new CreateEventController(accountService.Object, membershipService.Object,
                                                       formsAuthenticationService.Object, eventService.Object, null);
            controller.ModelState.AddModelError("*", "invalid model state");

            var result = controller.EventDetails(new CreateEventRequest()) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(controller.ModelState.IsValid, Is.False);
        }

        [Test]
        public void ValidTicketDetailsProvided_ReturnsRedirectToShareEvent()
        {
            var controller = new CreateEventController(accountService.Object,
                                                       membershipService.Object, formsAuthenticationService.Object,
                                                       eventService.Object, null);

            var result = controller.TicketDetails(new SetTicketDetailsRequest(){EventId=1}) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteName, Is.StringMatching("Event_ShareYourEvent"));
            Assert.That(result.RouteValues["id"], Is.EqualTo(1));
        }

        [Test]
        public void InvalidTicketDetailsProvided_ReturnsRedirectToShareEvent()
        {
            var controller = new CreateEventController(accountService.Object,
                                                       membershipService.Object, formsAuthenticationService.Object,
                                                       eventService.Object, null);
            controller.ModelState.AddModelError("*", "Invalid model state");

            var result = controller.TicketDetails(new SetTicketDetailsRequest()) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(controller.ModelState.IsValid, Is.False);
        }
    }
}
