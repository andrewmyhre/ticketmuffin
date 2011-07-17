using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmailProcessing;
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
        Mock<IEmailFacade> _emailFacade = new Mock<IEmailFacade>();

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
                .Setup(m=>m.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .Verifiable();
            IAccountService accountService = new AccountService(_accountRepository.Object, _emailFacade.Object);
            var createUserRequest = TestDataObjects.CreateValidCreateUserRequest();

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
            IAccountService accountService = new AccountService(_accountRepository.Object, _emailFacade.Object);
            var createUserRequest = TestDataObjects.CreateValidCreateUserRequest();

            // act
            accountService.CreateUser(createUserRequest);

            // assert
            _accountRepository.VerifyAll();
        }
    }
}
