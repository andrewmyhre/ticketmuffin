using System;
using GroupGiving.PayPal.Model;

namespace GroupGiving.PayPal
{
    public class HttpChannelException : Exception
    {
        public FaultMessage FaultMessage { get; set; }

        public HttpChannelException(FaultMessage faultMessage) : base(string.Format("{0}: {1}", faultMessage.Error.Parameter, faultMessage.Error.Message))
        {
            FaultMessage = faultMessage;
        }
    }
}