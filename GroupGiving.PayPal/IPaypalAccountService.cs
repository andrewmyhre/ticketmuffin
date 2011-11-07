using GroupGiving.PayPal.Model;

namespace GroupGiving.PayPal
{
    public interface IPaypalAccountService
    {
        VerifyPaypalAccountResponse VerifyPaypalAccount(VerifyPaypalAccountRequest request);
        CreatePaypalAccountResponse CreateAccount(CreatePayPalAccountRequest request);
    }
}