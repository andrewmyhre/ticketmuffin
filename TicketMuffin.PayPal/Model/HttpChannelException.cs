using System;

namespace GroupGiving.PayPal.Model
{
    public class HttpChannelException : Exception
    {
        public ResponseBase FaultMessage { get; set; }

        public HttpChannelException(FaultMessage faultMessage) : base(faultMessage.Error.Message)
        {
            FaultMessage = faultMessage;
        }

        public HttpChannelException(ResponseBase faultMessage)
            : base(faultMessage.ToString())
        {
            FaultMessage = faultMessage;
        }
    }
}