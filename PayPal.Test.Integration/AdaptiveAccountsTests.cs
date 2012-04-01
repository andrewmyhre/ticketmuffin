using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Domain;
using GroupGiving.PayPal.AdaptiveAccounts;
using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;
using NUnit.Framework;
using PayPal.Platform.SDK;
using PayPal.Services.Private.AA;
using log4net;

namespace PayPal.Test.Integration
{
    [TestFixture]
    public class AdaptiveAccountsTests
    {
        private ILog log = LogManager.GetLogger("tests");
        [SetUp]
        public void SetUp()
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Debug("log this");
        }

        [Test]
        public void Can_create_a_Paypal_Api_profile_from_configuration()
        {
            BaseAPIProfile profile = BaseApiProfileFactory.CreateFromConfiguration(ValidAccountApiConfiguration());
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.ApplicationID, Is.Not.Null);
            Assert.That(profile.APIUsername, Is.Not.Null);
            Assert.That(profile.APIPassword, Is.Not.Null);
            Assert.That(profile.Environment, Is.Not.Null);
        }

        private ISiteConfiguration ValidAccountApiConfiguration()
        {
            return new SiteConfiguration()
                       {
                           AdaptiveAccountsConfiguration = new AdaptiveAccountsConfiguration()
                            {
                                   SandboxMode = true,
                                   SandboxApiBaseUrl = "https://svcs.sandbox.paypal.com/",
                                   SandboxApplicationId = "APP-80W284485P519543T",
                                   DeviceIpAddress = "127.0.0.1",
                                   ApiUsername = "platfo_1255077030_biz_api1.gmail.com",
                                   ApiPassword = "1255077037",
                                   ApiSignature = "Abg0gYcQyxQvnf2HDJkKtA-p6pqhA1k-KTYE0Gcy1diujFio4io5Vqjf",
                                   RequestDataFormat = "XML",
                                   ResponseDataFormat = "XML",
                                   SandboxMailAddress = "something@something.com"
                            }
                       };
        }

        [TestCase("platfo11_per@gmail.com", "Bonzop", "Zaius")]
        [TestCase("seller_1304843436_biz@gmail.com", "Andrew", "Myhre")]
        public void Can_verify_an_account_which_exists(string email, string firstName, string lastName)
        {
            GetVerifiedStatusResponse getVerifiedStatusRes = null;

            BaseAPIProfile profile = BaseApiProfileFactory.CreateFromConfiguration(ValidAccountApiConfiguration());

            GetVerifiedStatusRequest getVerifiedStatusRequest = new GetVerifiedStatusRequest();
            getVerifiedStatusRequest.emailAddress = email;
            getVerifiedStatusRequest.firstName = firstName;
            getVerifiedStatusRequest.lastName = lastName;
            getVerifiedStatusRequest.matchCriteria = "NAME";
            
            AdaptiveAccountsSdk aa = new AdaptiveAccountsSdk();
            aa.APIProfile = profile;

            try
            {
                getVerifiedStatusRes = aa.GetVerifiedStatus(getVerifiedStatusRequest);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                while(ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception:");
                    ex = ex.InnerException;
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
                throw;
            }

            Assert.That(getVerifiedStatusRes, Is.Not.Null);
            Assert.That(getVerifiedStatusRes.responseEnvelope.ack, Is.EqualTo(AckCode.Success));
            Console.WriteLine("account status: {0}", getVerifiedStatusRes.accountStatus);
        }

        [Test]
        [Category("Slow")]
        public void Can_create_a_business_PayPal_account()
        {
            CreatePayPalAccountRequest request = new CreatePayPalAccountRequest();
            request.Salutation = "Mr.";
            request.FirstName = "Andrew";
            request.LastName = "Myhre";
            request.DateOfBirth = new DateTime(1980, 12, 22);
            request.ContactEmailAddress = "andrew.myhre."+Guid.NewGuid().ToString()+"@gmail.com";
            request.AddressLine1 = "122 Antill Road";
            request.AddressLine2 = "Hackney";
            request.City = "London";
            request.PostCode = "E35BN";
            request.CountryCode = "UK";
            request.CurrencyCode = "GBP";
            request.ContactPhoneNumber = "0123456789";
            request.MerchantWebsiteAddress = "http://www.ticketmuffin.com";
            request.OrganisationDateOfEstablisment = new DateTime(2001,01,01);

            BaseAPIProfile profile = BaseApiProfileFactory.CreateFromConfiguration(ValidAccountApiConfiguration());

            var service = new GroupGiving.PayPal.PaypalAccountService(ValidAccountApiConfiguration());

            var response = service.CreateAccount(request);

            Assert.That(response.Success, Is.True);
            Console.WriteLine(response.RedirectUrl);

        }
    }
}
