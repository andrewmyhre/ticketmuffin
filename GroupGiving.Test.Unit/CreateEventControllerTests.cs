using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;
using GroupGiving.Test.Common;
using GroupGiving.Web.Code;
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
        private Mock<IMembershipProviderLocator> _membershipProviderLocator = new Mock<IMembershipProviderLocator>();
        private Mock<IEventService> eventService = null;
        private Mock<MembershipProvider> _membershipProvider = new Mock<MembershipProvider>();
        private Mock<ICountryService> _countryService = new Mock<ICountryService>();
        private Mock<IPaypalAccountService> _paypalAccountService = new Mock<IPaypalAccountService>();
        private IIdentity _userIdentity = new System.Security.Principal.GenericIdentity("testuser@test.com");

        [SetUp]
        public void SetUp()
        {
            accountService = new Mock<IAccountService>();
            membershipService = new Mock<IMembershipService>();
            formsAuthenticationService = new Mock<IFormsAuthenticationService>();
            eventService = new Mock<IEventService>();
            _membershipProviderLocator
                .Setup(m => m.Provider())
                .Returns(_membershipProvider.Object);

            _countryService
                .Setup(m=>m.RetrieveAllCountries())
                .Returns(new[]
                       {
                           new Country("Poland"),
                           new Country("United Kingdom"), 
                           new Country("United States of America"),
                           new Country("New Zealand")
                       });

            accountService
                .Setup(m => m.RetrieveByEmailAddress(It.IsAny<string>()))
                .Returns(new Account() {Email = "testuser@test.com"});
        }

        [Test]
        public void ValidCreateEventDetailsProvided_ReturnsRedirectToTicketOptions()
        {
            EventCreationAlwaysSuccessful("test-event");
            AnyShortUrlIsAvailable();

            eventService
                .Setup(m => m.Retrieve(It.IsAny<string>()))
                .Returns(new GroupGivingEvent());

            _paypalAccountService
                .Setup(m => m.VerifyPaypalAccount(It.IsAny<VerifyPaypalAccountRequest>()))
                .Returns(new VerifyPaypalAccountResponse() { Success = true, AccountStatus = "Verified" });

            var controller = new CreateEventController(accountService.Object, _countryService.Object, membershipService.Object,
                                                       formsAuthenticationService.Object, eventService.Object,
                                                       null, _userIdentity, null, _membershipProviderLocator.Object, _paypalAccountService.Object);

            var createEventRequest = new CreateEventRequest(){ShortUrl="test-event"};
            createEventRequest.StartDate = DateTime.Now.AddDays(10).ToString("dd/MM/yyyy");
            createEventRequest.StartTime = "10:00PM";
            var result = controller.EventDetails(createEventRequest) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteName, Is.StringMatching("CreateEvent_TicketDetails"));
        }

        private void EventCreationAlwaysSuccessful(string shortUrl)
        {
            eventService
                .Setup(m => m.CreateEvent(It.IsAny<CreateEventRequest>()))
                .Returns(new CreateEventResult() {Success = true, Event = new GroupGivingEvent(){ShortUrl = shortUrl}});
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

            eventService
                .Setup(m => m.Retrieve(It.IsAny<string>()))
                .Returns(new GroupGivingEvent());

            _paypalAccountService
                .Setup(m => m.VerifyPaypalAccount(It.IsAny<VerifyPaypalAccountRequest>()))
                .Returns(new VerifyPaypalAccountResponse() { Success = true, AccountStatus = "Verified" });

            var controller = new CreateEventController(accountService.Object, _countryService.Object, membershipService.Object,
                                                       formsAuthenticationService.Object, eventService.Object,
                                                       null, _userIdentity, null, _membershipProviderLocator.Object, _paypalAccountService.Object);
            controller.ModelState.AddModelError("*", "invalid model state");

            var result = controller.EventDetails(new CreateEventRequest()) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(controller.ModelState.IsValid, Is.False);
        }

        [Test]
        [Ignore("find an alternative to membership provider")]
        public void ValidTicketDetailsProvided_ReturnsRedirectToShareEvent()
        {
            var groupGivingEvent = new GroupGivingEvent() {ShortUrl = "shorturl"};  
            eventService
                .Setup(m => m.Retrieve(It.IsAny<int>()))
                .Returns(groupGivingEvent);

            eventService
                .Setup(m => m.Retrieve(It.IsAny<string>()))
                .Returns(new GroupGivingEvent());

            _paypalAccountService
                .Setup(m => m.VerifyPaypalAccount(It.IsAny<VerifyPaypalAccountRequest>()))
                .Returns(new VerifyPaypalAccountResponse() { Success = true, AccountStatus = "Verified" });
            
            var controller = new CreateEventController(accountService.Object, _countryService.Object,
                                                       membershipService.Object, formsAuthenticationService.Object,
                                                       eventService.Object, null, _userIdentity, null, _membershipProviderLocator.Object, _paypalAccountService.Object);

            var setTicketDetailsRequest = new SetTicketDetailsRequest() {ShortUrl="test-event"};
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
            _paypalAccountService
                .Setup(m => m.VerifyPaypalAccount(It.IsAny<VerifyPaypalAccountRequest>()))
                .Returns(new VerifyPaypalAccountResponse() {Success = true, AccountStatus = "Verified"});
                    
            var controller = new CreateEventController(accountService.Object, _countryService.Object,
                                                       membershipService.Object, formsAuthenticationService.Object,
                                                       eventService.Object, null, _userIdentity, null, _membershipProviderLocator.Object, _paypalAccountService.Object);
            controller.ModelState.AddModelError("*", "Invalid model state");
            eventService
                .Setup(m => m.Retrieve(It.IsAny<string>()))
                .Returns(new GroupGivingEvent());
            var result = controller.TicketDetails(new SetTicketDetailsRequest()) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(controller.ModelState.IsValid, Is.False);
        }
    }
}
