using System.Linq;
using EmailProcessing;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using Moq;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace GroupGiving.Test.Unit
{
    [TestFixture]
    public class GivenCreatingAnAccount : InMemoryStoreTest
    {
        Mock<IRepository<Account>> _accountRepository = new Mock<IRepository<Account>>();
        Mock<IEmailFacade> _emailFacade = new Mock<IEmailFacade>();
        private EmbeddableDocumentStore _documentStore;

        [SetUp]
        public void SetUp()
        {
            _documentStore = InMemoryStore();
        }

        [Test]
        [Ignore("Can't create expectation on a method with an anonymous type argument (can i??)")]
        public void WhenRequestIsValid_ThankYouEmailIsSentToUser()
        {
            using (var session = _documentStore.OpenSession())
            {
                // arrange
                _emailFacade
                    .Setup(m => m.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), "pl"))
                    .Verifiable();
                IAccountService accountService = new AccountService(_emailFacade.Object, session);
                var createUserRequest = Helpers.CreateValidCreateUserRequest();

                // act
                accountService.CreateUser(createUserRequest);

                // assert
                _emailFacade.Verify();
            }
        }

        [Test]
        public void WhenRequestIsValid_AccountObjectIsSaved()
        {
            using (var session = _documentStore.OpenSession())
            {
                // arrange
                IAccountService accountService = new AccountService(_emailFacade.Object, session);
                var createUserRequest = Helpers.CreateValidCreateUserRequest();

                // act
                accountService.CreateUser(createUserRequest);

                // assert
                var account = session.Query<Account>().SingleOrDefault(a => a.Email == createUserRequest.Email);
                Assert.That(account, Is.Not.Null);
            }
        }
    }
}
