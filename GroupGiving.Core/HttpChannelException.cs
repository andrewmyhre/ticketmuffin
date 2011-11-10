using System;
using GroupGiving.Core.Dto;

namespace GroupGiving.Core
{
    public class HttpChannelException : Exception
    {
        public ResponseBase FaultMessage { get; set; }

        public HttpChannelException(ResponseBase faultMessage)
            : base(faultMessage.ToString())
        {
            FaultMessage = faultMessage;
        }
    }
}