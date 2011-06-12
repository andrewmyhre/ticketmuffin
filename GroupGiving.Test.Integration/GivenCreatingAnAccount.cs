using System;
using System.Collections.Generic;
using System.IO;
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
using Raven.Client.Document;

namespace GroupGiving.Test.Integration
{
    [TestFixture]
    public class GivenCreatingAnAccount
    {
        private IDocumentStore storage = null;
        private IEmailCreationService _emailCreationService = null;

        [SetUp]
        public void Setup()
        {
            string storagePath = Path.Combine(Environment.CurrentDirectory, "ravenStore");
            if (Directory.Exists(storagePath))
            Directory.Delete(storagePath);
            storage = new Raven.Client.Embedded.EmbeddableDocumentStore()
                          {
                              DataDirectory = storagePath
                          };
            storage.Initialize();

            
        }

        [Test]
        public void WhenRequestIsValid_ThenUserAccountIsCreated()
        {
            // arrange
            using (var session = storage.OpenSession())
            {
                IRepository<Account> accountRepository = new RavenDBRepositoryBase<Account>(session);
                IAccountService accountService = new AccountService(accountRepository, _emailCreationService, new Mock<IEmailRelayService>().Object);

                CreateUserRequest createUserRequest = TestDataObjects.CreateValidCreateUserRequest();
                // act
                var account = accountService.CreateUser(createUserRequest);

                // assert
                var assertAccount = session.Load<Account>(account.Id);
                Assert.That(assertAccount, Is.Not.Null);
            }
        }
    }
}
