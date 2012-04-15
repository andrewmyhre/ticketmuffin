using GroupGiving.PayPal.Clients;
using GroupGiving.PayPal.Model;
using Moq;

namespace GroupGiving.Test.Unit
{
    public static class ApiMockExtensions
    {
        public static Mock<IAccountsApiClient> AllAccountsVerified(this Mock<IAccountsApiClient> accountsClient)
        {
            accountsClient
                .Setup(a => a.VerifyAccount(It.IsAny<GetVerifiedStatusRequest>()))
                .Returns(new GetVerifiedStatusResponse()
                             {
                                 Success = true,
                                 Verified = true,
                                 AccountStatus = "VERIFIED"
                             });

            return accountsClient;

        }
    }
}