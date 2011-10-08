using System;
using System.Collections.Generic;
using System.IO;
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
using Raven.Client.Document;

namespace GroupGiving.Test.Integration
{
    [TestFixture]
    public class GivenCreatingAnAccount : ApplicationTestsBase
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void WhenRequestIsValid_ThenUserAccountIsCreated()
        {
            // arrange
            using (var session = _storage.OpenSession())
            {
                try
                {
                    IRepository<Account> accountRepository = new RavenDBRepositoryBase<Account>(session);
                    IAccountService accountService = new AccountService(accountRepository, _emailService);

                    CreateUserRequest createUserRequest = TestDataObjects.CreateValidCreateUserRequest();
                    // act
                    var account = accountService.CreateUser(createUserRequest);

                    // assert
                    var assertAccount = session.Load<Account>(account.Id);
                    Assert.That(assertAccount, Is.Not.Null);
                } finally
                {
                }
            }
        }
    }
}
