﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using GroupGiving.PayPal.AdaptiveAccounts;
using GroupGiving.PayPal.Configuration;
using NUnit.Framework;
using PayPal.Platform.SDK;
using PayPal.Services.Private.AA;
using log4net;
using AdaptiveAccounts = PayPal.Platform.SDK.AdaptiveAccounts;

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
        public void Can_read_adaptive_payments_configuration()
        {
            var configuration =
                ConfigurationManager.GetSection("adaptiveAccounts") as PaypalAdaptiveAccountsConfigurationSection;

            Assert.That(configuration, Is.Not.Null);
            Assert.That(configuration.ApiPassword, Is.Not.Null);
            Assert.That(configuration.ApiUsername, Is.Not.Null);
            Assert.That(configuration.ApiSignature, Is.Not.Null);
            Assert.That(configuration.ApplicationId, Is.Not.Null);
            Assert.That(configuration.Environment, Is.Not.Null);
        }

        [Test]
        public void Can_create_a_Paypal_Api_profile_from_configuration()
        {
            var configuration =
                ConfigurationManager.GetSection("adaptiveAccounts") as PaypalAdaptiveAccountsConfigurationSection;

            BaseAPIProfile profile = BaseApiProfileFactory.CreateFromConfiguration(configuration);
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.ApplicationID, Is.Not.Null);
            Assert.That(profile.APIUsername, Is.Not.Null);
            Assert.That(profile.APIPassword, Is.Not.Null);
            Assert.That(profile.Environment, Is.Not.Null);
        }

        [TestCase("platfo11_per@gmail.com", "Bonzop", "Zaius")]
        [TestCase("seller_1304843436_biz@gmail.com", "Andrew", "Myhre")]
        public void Can_verify_an_account_which_exists(string email, string firstName, string lastName)
        {
            GetVerifiedStatusResponse getVerifiedStatusRes = null;

            var configuration =
    ConfigurationManager.GetSection("adaptiveAccounts") as PaypalAdaptiveAccountsConfigurationSection;

            BaseAPIProfile profile = BaseApiProfileFactory.CreateFromConfiguration(configuration);

            GetVerifiedStatusRequest getVerifiedStatusRequest = new GetVerifiedStatusRequest();
            getVerifiedStatusRequest.emailAddress = email;
            getVerifiedStatusRequest.firstName = firstName;
            getVerifiedStatusRequest.lastName = lastName;
            getVerifiedStatusRequest.matchCriteria = "NAME";
            
            AdaptiveAccounts aa = new AdaptiveAccounts();
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
    }
}
