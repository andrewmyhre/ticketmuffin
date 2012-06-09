using TicketMuffin.PayPal.Model;

namespace TicketMuffin.PayPal.Clients
{
    public class AccountsClient : IAccountsApiClient
    {
        private readonly ApiClientSettings _clientSettings;
        public string AdaptiveAccountsUrl { get { return _clientSettings.Configuration.ApiBaseUrl + "/AdaptiveAccounts"; } }

        public AccountsClient(ApiClientSettings clientSettings)
        {
            _clientSettings = clientSettings;
        }
        public GetVerifiedStatusResponse VerifyAccount(GetVerifiedStatusRequest request)
        {
            return new HttpChannel().ExecuteRequest<GetVerifiedStatusRequest, GetVerifiedStatusResponse>("AdaptiveAccounts", "GetVerifiedStatus", request, _clientSettings);
        }

        public RequestPermissionsResponse RequestPermissions(RequestPermissionsRequest request)
        {
            return new HttpChannel().ExecuteRequest<RequestPermissionsRequest, RequestPermissionsResponse>
                ("Permissions", "RequestPermissions", request, _clientSettings);
        }

        public GetAccessTokenResponse GetAccessToken(GetAccessTokenRequest request)
        {
            return new HttpChannel().ExecuteRequest<GetAccessTokenRequest, GetAccessTokenResponse>
                ("Permissions", "GetAccessToken", request, _clientSettings);
        }
    }
}