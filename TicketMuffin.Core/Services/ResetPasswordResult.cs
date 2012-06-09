namespace TicketMuffin.Core.Services
{
    public class ResetPasswordResult
    {
        public bool Success { get; set; }
        public bool InvalidToken { get; set; }

        public static ResetPasswordResult InvalidTokenResult
        {
            get
            {
                return new ResetPasswordResult() { InvalidToken = true };
            }
        }

        public static ResetPasswordResult SuccessResult
        {
            get { return new ResetPasswordResult(){Success=true};}
        }

    }
}