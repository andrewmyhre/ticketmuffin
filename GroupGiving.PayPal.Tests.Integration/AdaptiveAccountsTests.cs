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

        [Test]
        public void Can_get_a_RequestPermissions_token()
        {
            _paypalConfiguration = PayPalHelpers.SandboxConfiguration();
            _apiSettings = new ApiClientSettings(_paypalConfiguration);
            _apiClient = new ApiClient(_apiSettings, new SiteConfiguration(){AdaptiveAccountsConfiguration = _paypalConfiguration});

            RequestPermissionsRequest request = new RequestPermissionsRequest(_paypalConfiguration)
            {
                Scope = new string[] { "DIRECT_PAYMENT", "MASS_PAY", "TRANSACTION_DETAILS","REFUND", },
                Callback = "http://localhost/PayPal/Permissions"
            };

            var response = _apiClient.RequestPermissions(request);
            Assert.That(response.Token, Is.Not.Null);
            Assert.That(response.Token, Has.Length.GreaterThan(0));
            System.Diagnostics.Debug.WriteLine(string.Format("sandbox: https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_grant-permission&request_token={0}", response.Token));
            System.Diagnostics.Debug.WriteLine(string.Format("live: https://www.paypal.com/cgi-bin/webscr?cmd=_grant-permission&request_token={0}", response.Token));

        }

        [Test]
        public void Can_get_accesstoken()
        {
            _paypalConfiguration = PayPalHelpers.SandboxConfiguration();
            _apiSettings = new ApiClientSettings(_paypalConfiguration);
            _apiClient = new ApiClient(_apiSettings, new SiteConfiguration() { AdaptiveAccountsConfiguration = _paypalConfiguration });

            string verificationCode = "EF8MjKKO35.aOsjvl.9myg";
            var getACcessTokenRequest = new GetAccessTokenRequest()
            {
                Verifier = "EF8MjKKO35.aOsjvl.9myg",
                Token = "AAAAAAAVsJJs368CZSYT"
            };
            var getACcessTOkenResponse = _apiClient.GetAccessToken(getACcessTokenRequest);

            Assert.That(getACcessTOkenResponse.Token, Is.Not.Null);
            Assert.That(getACcessTOkenResponse.TokenSecret, Is.Not.Null);
        }
    }
}