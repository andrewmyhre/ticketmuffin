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
    [Ignore("embedded ravendb storage doesn't work properly, sets readonly after first run, subsequent runs fail with IO exception")]
    public class GivenCreatingAnAccount
    {
        private IDocumentStore storage = null;
        private Mock<IEmailFacade> _emailFacade = null;
        private Mock<IEmailRelayService> _emailRelayService;

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

            _emailFacade = new Mock<IEmailFacade>();
        }

        [Test]
        public void WhenRequestIsValid_ThenUserAccountIsCreated()
        {
            // arrange
            using (var session = storage.OpenSession())
            {
                try
                {
                    IRepository<Account> accountRepository = new RavenDBRepositoryBase<Account>(session);
                    _emailRelayService = new Mock<IEmailRelayService>();
                    IAccountService accountService = new AccountService(accountRepository, _emailFacade.Object);

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
