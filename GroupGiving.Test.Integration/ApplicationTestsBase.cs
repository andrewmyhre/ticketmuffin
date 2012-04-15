using EmailProcessing;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using Moq;
using Raven.Client;
using Raven.Client.Document;

namespace GroupGiving.Test.Integration
{
    public class ApplicationTestsBase
    {
        protected Mock<IEventService> _eventService = new Mock<IEventService>();
        protected IEmailFacade _emailService;
        protected Mock<IAccountService> _accountService = new Mock<IAccountService>();
        protected Mock<ICountryService> _countryService = new Mock<ICountryService>();
        protected Mock<IDocumentStore> _documentStore = new Mock<IDocumentStore>();
        protected Mock<IDocumentSession> _documentSession = new Mock<IDocumentSession>();
        
        protected IDocumentStore _storage;

        public ApplicationTestsBase()
        {
            _emailService = new Mock<IEmailFacade>().Object;
            _storage = new DocumentStore()
                           {
                               Url = "http://localhost:8080"
                           };
            _storage.Initialize();

            _eventService
                .Setup(x => x.ShortUrlAvailable("availableUrl"))
                .Returns(true);
            _eventService
                .Setup(x => x.ShortUrlAvailable("takenUrl"))
                .Returns(false);
            _documentStore
                .Setup(m => m.OpenSession())
                .Returns(_documentSession.Object);
        }

        protected void SetAccountServiceToReturn(Account account)
        {
            _accountService
                .Setup(x => x.RetrieveByEmailAddress(It.IsAny<string>()))
                .Returns(account);

        }

        public Account CreateTestUserAccount(IDocumentSession session)
        {
            IRepository<Account> accountRepository = new RavenDBRepositoryBase<Account>(session);
            IAccountService accountService = new AccountService(accountRepository, _emailService, _documentStore.Object);

            CreateUserRequest createUserRequest = Helpers.CreateValidCreateUserRequest();
            // act
            var account = accountService.CreateUser(createUserRequest);
            return account;

        }
    }
}