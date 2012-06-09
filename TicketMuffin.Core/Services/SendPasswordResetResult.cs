namespace TicketMuffin.Core.Services
{
    public class SendPasswordResetResult
    {
        public bool AccountNotFound { get; set; }
        public bool Successful { get; set; }

        public static SendPasswordResetResult AccountNotFoundResult
        {
            get { return new SendPasswordResetResult() {AccountNotFound = true}; }
        }
        public static SendPasswordResetResult SuccessResult
        {
            get { return new SendPasswordResetResult() { Successful = true }; }
        }

    }
}