namespace TicketMuffin.Core.Services
{
    public class SendThanksForRegisteringEmailResult
    {
        public bool Success { get; set; }
        public static SendThanksForRegisteringEmailResult SuccessResult
        {
            get { return new SendThanksForRegisteringEmailResult() { Success = true }; }
        }
    }
}