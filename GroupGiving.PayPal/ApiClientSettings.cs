namespace GroupGiving.PayPal
{
    public class ApiClientSettings
    {
        public ApiClientSettings()
        {
            ApiEndpointBase = "https://svcs.sandbox.paypal.com/AdaptivePayments";
            PayApiEndpoint = ApiEndpointBase+"/Pay";
            ApiVersion = "1.1.0";
            RequestDataBinding = "XML";
            ResponseDataBinding = "XML";
        }


        public ApiClientSettings(string username, string password, string signature) : this()
        {
            this.Username = username;
            this.Password = password;
            this.Signature = signature;
        }

        protected string ApiEndpointBase { get; set; }
        
        public string Username { get; set; }
        public string Password { get; set; }
        public string Signature { get; set; }
        public string ApiVersion { get; set; }
        public string RequestDataBinding { get; set; }
        public string ResponseDataBinding { get; set; }

        public string PayApiEndpoint { get; set; }

        public string ActionUrl(string action)
        {
            return ApiEndpointBase + "/" + action;
        }
    }
}