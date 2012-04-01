using GroupGiving.Core.Configuration;
using GroupGiving.Core.Domain;
using GroupGiving.PayPal.Configuration;

namespace GroupGiving.PayPal
{
    public class ApiClientSettings
    {
        private readonly AdaptiveAccountsConfiguration _paypalConfiguration;

        public ApiClientSettings(AdaptiveAccountsConfiguration paypalConfiguration)
        {
            _paypalConfiguration = paypalConfiguration;

            ApiEndpointBase = paypalConfiguration.ApiBaseUrl;
            ApiVersion = paypalConfiguration.ApiVersion;
            RequestDataBinding = paypalConfiguration.RequestDataBinding;
            ResponseDataBinding = paypalConfiguration.ResponseDataBinding;
            Username = paypalConfiguration.ApiUsername;
            Password = paypalConfiguration.ApiPassword;
            Signature = paypalConfiguration.ApiSignature;
            ApplicationId = paypalConfiguration.ApplicationId;

        }

        protected string ApiEndpointBase { get; set; }
        
        public string Username { get; set; }
        public string Password { get; set; }
        public string Signature { get; set; }
        public string ApiVersion { get; set; }
        public string RequestDataBinding { get; set; }
        public string ResponseDataBinding { get; set; }

        public string PayApiEndpoint { get; set; }

        public string ApplicationId { get; set; }

        public string ActionUrl(string api, string action)
        {
            return ApiEndpointBase + "/" + api + "/" + action;
        }
    }
}