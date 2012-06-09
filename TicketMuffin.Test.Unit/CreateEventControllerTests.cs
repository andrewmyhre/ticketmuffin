using System;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Raven.Client.Embedded;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal.Clients;
using TicketMuffin.Web.Controllers;
using TicketMuffin.Web.Models;

namespace GroupGiving.Test.Unit
{
    [TestFixture]
    public class CreateEventControllerTests : PaymentGatewayTestsBase
    {
        Mock<IAccountService> accountService = null;
        Mock<IMembershipService> membershipService = null;
        Mock<IFormsAuthenticationService> formsAuthenticationService = null;
        private Mock<IEventService> eventService = null;
        private Mock<MembershipProvider> _membershipProvider = new Mock<MembershipProvider>();
        private Mock<ICountryService> _countryService = new Mock<ICountryService>();
        private IIdentity _userIdentity = new System.Security.Principal.GenericIdentity("testuser@test.com");
        private Mock<IApiClient> _apiClient = new Mock<IApiClient>();
        private Mock<IAccountsApiClient> _accountsApi = new Mock<IAccountsApiClient>();
        private EmbeddableDocumentStore _documentStore;
        private Mock<MembershipProvider> _membershipProviderLocator = new Mock<MembershipProvider>();

        [SetUp]
        public void SetUp()
        {
            _documentStore = InMemoryStore();

            accountService = new Mock<IAccountService>();
            membershipService = new Mock<IMembershipService>();
            formsAuthenticationService = new Mock<IFormsAuthenticationService>();
            eventService = new Mock<IEventService>();

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

            _apiClient.SetupProperty(a => a.Accounts, _accountsApi.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _documentStore.Dispose();
        }

        [Test]
        public void ValidCreateEventDetailsProvided_ReturnsRedirectToTicketOptions()
        {
            using (var session = _documentStore.OpenSession())
            {
                EventCreationAlwaysSuccessful("test-event");
                AnyShortUrlIsAvailable();

                eventService
                    .Setup(m => m.Retrieve(It.IsAny<string>()))
                    .Returns(new GroupGivingEvent());

                _accountsApi.AllAccountsVerified();

                var controller = new CreateEventController(accountService.Object, _countryService.Object,
                                                           membershipService.Object,
                                                           formsAuthenticationService.Object, eventService.Object,
                                                           _userIdentity, _membershipProviderLocator.Object,
                                                           _apiClient.Object, session);

                var createEventRequest = new CreateEventRequest() {ShortUrl = "test-event"};
                createEventRequest.StartDate = DateTime.Now.AddDays(10).ToString("dd/MM/yyyy");
                createEventRequest.StartTime = "10:00PM";
                var result = controller.EventDetails(createEventRequest) as RedirectToRouteResult;

                Assert.That(result, Is.Not.Null);
                Assert.That(result.RouteName, Is.StringMatching("CreateEvent_TicketDetails"));
            }
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
            using (var session = _documentStore.OpenSession())
            {
                eventService
                    .Setup(m => m.CreateEvent(It.IsAny<CreateEventRequest>()))
                    .Returns(new CreateEventResult() {Success = false});

                eventService
                    .Setup(m => m.Retrieve(It.IsAny<string>()))
                    .Returns(new GroupGivingEvent());

                _accountsApi.AllAccountsVerified();

                var controller = new CreateEventController(accountService.Object, _countryService.Object,
                                                           membershipService.Object,
                                                           formsAuthenticationService.Object, eventService.Object,
                                                           _userIdentity, _membershipProviderLocator.Object,
                                                           _apiClient.Object, session);
                controller.ModelState.AddModelError("*", "invalid model state");

                var result = controller.EventDetails(new CreateEventRequest()) as ViewResult;

                Assert.That(result, Is.Not.Null);
                Assert.That(controller.ModelState.IsValid, Is.False);
            }
        }

        [Test]
        [Ignore("find an alternative to membership provider")]
        public void ValidTicketDetailsProvided_ReturnsRedirectToShareEvent()
        {
            using (var session = _documentStore.OpenSession())
            {
                var groupGivingEvent = new GroupGivingEvent() {ShortUrl = "shorturl"};
                eventService
                    .Setup(m => m.Retrieve(It.IsAny<int>()))
                    .Returns(groupGivingEvent);

                eventService
                    .Setup(m => m.Retrieve(It.IsAny<string>()))
                    .Returns(new GroupGivingEvent());

                _accountsApi.AllAccountsVerified();

                var controller = new CreateEventController(accountService.Object, _countryService.Object,
                                                           membershipService.Object, formsAuthenticationService.Object,
                                                           eventService.Object, _userIdentity,
                                                           _membershipProviderLocator.Object, _apiClient.Object, session);

                var setTicketDetailsRequest = new SetTicketDetailsRequest() {ShortUrl = "test-event"};
                setTicketDetailsRequest.SalesEndDate = DateTime.Now.AddDays(10).ToString("dd/MM/yyyy");
                setTicketDetailsRequest.SalesEndTime = "10:00PM";
                var result = controller.TicketDetails(setTicketDetailsRequest) as RedirectToRouteResult;

                Assert.That(result, Is.Not.Null);
                Assert.That(result.RouteName, Is.StringMatching("Event_ShareYourEvent"));
                Assert.That(result.RouteValues["shortUrl"], Is.EqualTo(groupGivingEvent.ShortUrl));
            }
        }

        [Test]
        public void InvalidTicketDetailsProvided_ReturnsRedirectToShareEvent()
        {
            using (var session = _documentStore.OpenSession())
            {
                _accountsApi.AllAccountsVerified();

                var controller = new CreateEventController(accountService.Object, _countryService.Object,
                                                           membershipService.Object, formsAuthenticationService.Object,
                                                           eventService.Object, _userIdentity,
                                                           _membershipProviderLocator.Object, _apiClient.Object, session);
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
}
