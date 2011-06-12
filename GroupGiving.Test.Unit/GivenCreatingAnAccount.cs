using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using GroupGiving.Test.Common;
using Moq;
using NUnit.Framework;
using Raven.Client;

namespace GroupGiving.Test.Unit
{
    [TestFixture]
    public class GivenCreatingAnAccount
    {
        Mock<IRepository<Account>> _accountRepository = new Mock<IRepository<Account>>();
        Mock<IEmailCreationService> _emailCreationService = new Mock<IEmailCreationService>();
        Mock<IEmailRelayService> _emailRelayService = new Mock<IEmailRelayService>();

        [SetUp]
        public void SetUp()
        {
            
        }

        [Test]
        public void WhenRequestIsValid_ThankYouEmailIsSentToUser()
        {
            // arrange
            _emailCreationService
                .Setup(m=>m.ThankYouForRegisteringEmail(It.IsAny<string>(), It.IsAny<string>()))
                .Verifiable();
            IAccountService accountService = new AccountService(_accountRepository.Object, _emailCreationService.Object, _emailRelayService.Object);
            var createUserRequest = TestDataObjects.CreateValidCreateUserRequest();

            // act
            accountService.CreateUser(createUserRequest);

            // assert
            _emailCreationService.Verify(m=>m.ThankYouForRegisteringEmail(It.IsAny<string>(), It.IsAny<string>()));
        }

        [Test]
        public void WhenRequestIsValid_AccountObjectIsSaved()
        {
            // arrange
            _accountRepository
                .Setup(m=>m.SaveOrUpdate(It.IsAny<Account>()))
                .Verifiable();
            IAccountService accountService = new AccountService(_accountRepository.Object, _emailCreationService.Object, _emailRelayService.Object);
            var createUserRequest = TestDataObjects.CreateValidCreateUserRequest();

            // act
            accountService.CreateUser(createUserRequest);

            // assert
            _accountRepository.VerifyAll();
        }
    }
}
