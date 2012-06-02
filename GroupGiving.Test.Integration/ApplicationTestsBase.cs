using EmailProcessing;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using Moq;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace GroupGiving.Test.Integration
{
    public class ApplicationTestsBase
    {
        protected Mock<IEventService> _eventService = new Mock<IEventService>();
        protected IEmailFacade _emailService;
        protected Mock<IAccountService> _accountService = new Mock<IAccountService>();
        protected Mock<ICountryService> _countryService = new Mock<ICountryService>();

        protected IDocumentStore _documentStore;

        [SetUp]
        public void SetUp()
        {
            _emailService = new Mock<IEmailFacade>().Object;
            _documentStore = new EmbeddableDocumentStore() {RunInMemory = true}.Initialize();

            _eventService
                .Setup(x => x.ShortUrlAvailable("availableUrl"))
                .Returns(true);
            _eventService
                .Setup(x => x.ShortUrlAvailable("takenUrl"))
                .Returns(false);
        }

        [TearDown]
        public void TearDown()
        {
            _documentStore.Dispose();
        }

        protected void SetAccountServiceToReturn(Account account)
        {
            _accountService
                .Setup(x => x.RetrieveByEmailAddress(It.IsAny<string>()))
                .Returns(account);

        }

        public Account CreateTestUserAccount(IDocumentSession session)
        {
            IAccountService accountService = new AccountService(_emailService, session);

            CreateUserRequest createUserRequest = Helpers.CreateValidCreateUserRequest();
            // act
            var account = accountService.CreateUser(createUserRequest);
            return account;

        }
    }
}