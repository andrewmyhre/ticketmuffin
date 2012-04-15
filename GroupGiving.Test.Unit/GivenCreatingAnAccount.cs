using EmailProcessing;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using Moq;
using NUnit.Framework;
using Raven.Client;

namespace GroupGiving.Test.Unit
{
    [TestFixture]
    public class GivenCreatingAnAccount
    {
        Mock<IRepository<Account>> _accountRepository = new Mock<IRepository<Account>>();
        Mock<IEmailFacade> _emailFacade = new Mock<IEmailFacade>();
        Mock<IDocumentStore> _documentStore = new Mock<IDocumentStore>();

        [SetUp]
        public void SetUp()
        {
            
        }

        [Test]
        [Ignore("Can't create expectation on a method with an anonymous type argument (can i??)")]
        public void WhenRequestIsValid_ThankYouEmailIsSentToUser()
        {
            // arrange
            _emailFacade
                .Setup(m=>m.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), "pl"))
                .Verifiable();
            IAccountService accountService = new AccountService(_accountRepository.Object, _emailFacade.Object, _documentStore.Object);
            var createUserRequest = Helpers.CreateValidCreateUserRequest();

            // act
            accountService.CreateUser(createUserRequest);

            // assert
            _emailFacade.Verify();
        }

        [Test]
        public void WhenRequestIsValid_AccountObjectIsSaved()
        {
            // arrange
            _accountRepository
                .Setup(m=>m.SaveOrUpdate(It.IsAny<Account>()))
                .Verifiable();
            IAccountService accountService = new AccountService(_accountRepository.Object, _emailFacade.Object, _documentStore.Object);
            var createUserRequest = Helpers.CreateValidCreateUserRequest();

            // act
            accountService.CreateUser(createUserRequest);

            // assert
            _accountRepository.VerifyAll();
        }
    }
}
