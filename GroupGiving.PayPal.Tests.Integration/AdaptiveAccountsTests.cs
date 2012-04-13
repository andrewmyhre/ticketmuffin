using System.IO;
using System.Net;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.PayPal;
using NUnit.Framework;

namespace GroupGiving.PayPal.Tests.Integration
{
    [TestFixture]
    public class AdaptiveAccountsTests
    {
        private ApiClientSettings _apiSettings;
        private AdaptiveAccountsConfiguration _paypalConfiguration;
        private IApiClient _apiClient;

        [Test]
        public void CanVerifyAPaypalAccount()
        {
            _paypalConfiguration = PayPalHelpers.SandboxConfiguration();
            _apiSettings = new ApiClientSettings(_paypalConfiguration);
            _apiClient = new ApiClient(_apiSettings, new SiteConfiguration { AdaptiveAccountsConfiguration = _paypalConfiguration });

            GetVerifiedStatusRequest request=new GetVerifiedStatusRequest(_paypalConfiguration)
            {
                EmailAddress = "Raiser_1321277286_biz@gmail.com",
                FirstName = "Andrzej",
                LastName = "Wieckowski"
            };
            try
            {
                var response = _apiClient.VerifyAccount(request);
                Assert.That(response, Is.Not.Null);
                Assert.That(response.AccountStatus, Is.StringContaining("VERIFIED"));
            } catch (WebException ex)
            {
                using (var stream =ex.Response.GetResponseStream())
                using (var streamReader = new StreamReader(stream))
                {
                    System.Diagnostics.Debug.Write(streamReader.ReadToEnd());
                }
            }
        }
    }
}