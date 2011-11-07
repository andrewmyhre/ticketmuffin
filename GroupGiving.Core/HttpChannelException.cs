using System;

namespace GroupGiving.Core
{
    public class HttpChannelException : Exception
    {
        public object FaultMessage { get; set; }

        public HttpChannelException(object faultMessage)
            : base(faultMessage.ToString())
        {
            FaultMessage = faultMessage;
        }
    }
}