using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Ninject;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;

namespace TicketMuffin.Core.Test.Integration
{
    [TestFixture]
    public class AuthenticationServiceTests : TicketMuffinTestsBase
    {
        private StandardKernel _kernel;

        [SetUp]
        public void Setup()
        {
            _kernel = new StandardKernel(new TicketMuffin.Core.Conventions.IoCConfiguration(),
                                            new TicketMuffin.Core.Test.Integration.TestNinjectModule());


        }

        [Test]
        public void CanCreateCredentials()
        {
            var documentStore = _kernel.Get<IDocumentStore>();
            var authService = _kernel.Get<IAuthenticationService>();

            string username = RandomString();
            string password = RandomString();

            authService.CreateCredentials(username, password, password);

            using (var session = documentStore.OpenSession())
            {
                var credentials = session.Query<Credentials>().SingleOrDefault(c => c.Username == username);
                Assert.That(credentials, Is.Not.Null);
                Assert.That(BCrypt.CheckPassword(password, Encoding.UTF8.GetString(credentials.SaltedHashedPassword)), Is.True);
            }
        }

        [Test]
        public void WhenPasswordsDoNotMatch_CreateCredentialsFails()
        {
            var documentStore = _kernel.Get<IDocumentStore>();
            var authService = _kernel.Get<IAuthenticationService>();

            string username = RandomString();
            string password = RandomString();
            string confirmPassword = RandomString();

            var exception = Assert.Throws<Exception>(() => authService.CreateCredentials(username, password, confirmPassword));
            Assert.That(exception.Message, Is.StringMatching("The passwords do not match"));
        }
    }
}
