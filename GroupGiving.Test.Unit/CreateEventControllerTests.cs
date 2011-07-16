using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using GroupGiving.Core.Domain;
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
            EventCreationAlwaysSuccessful();
            AnyShortUrlIsAvailable();

            var controller = new CreateEventController(accountService.Object, membershipService.Object,
                                                       formsAuthenticationService.Object, eventService.Object, null);

            var createEventRequest = new CreateEventRequest();
            createEventRequest.StartDate = DateTime.Now.AddDays(10).ToString("dd/MM/yyyy");
            createEventRequest.StartTime = "10:00PM";
            var result = controller.EventDetails(createEventRequest) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteName, Is.StringMatching("CreateEvent_TicketDetails"));
        }

        private void EventCreationAlwaysSuccessful()
        {
            eventService
                .Setup(m => m.CreateEvent(It.IsAny<CreateEventRequest>()))
                .Returns(new CreateEventResult() {Success = true});
        }

        private void AnyShortUrlIsAvailable()
        {
            eventService
                .Setup(m => m.ShortUrlAvailable(It.IsAny<string>()))
                .Returns(true);
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
            var groupGivingEvent = new GroupGivingEvent() {ShortUrl = "shorturl"};  
            eventService
                .Setup(m => m.Retrieve(It.IsAny<int>()))
                .Returns(groupGivingEvent);

            var controller = new CreateEventController(accountService.Object,
                                                       membershipService.Object, formsAuthenticationService.Object,
                                                       eventService.Object, null);

            var setTicketDetailsRequest = new SetTicketDetailsRequest() {EventId = 1};
            setTicketDetailsRequest.SalesEndDate = DateTime.Now.AddDays(10).ToString("dd/MM/yyyy");
            setTicketDetailsRequest.SalesEndTime = "10:00PM";
            var result = controller.TicketDetails(setTicketDetailsRequest) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteName, Is.StringMatching("Event_ShareYourEvent"));
            Assert.That(result.RouteValues["shortUrl"], Is.EqualTo(groupGivingEvent.ShortUrl));
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
