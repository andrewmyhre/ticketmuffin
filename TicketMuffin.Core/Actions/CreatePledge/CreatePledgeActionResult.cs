using System;
using TicketMuffin.PayPal.Model;

namespace TicketMuffin.Core.Actions.CreatePledge
{
    public class CreatePledgeActionResult
    {
        public IPaymentGatewayResponse GatewayResponse { get; set; }

        public bool Succeeded { get; set; }

        public Exception Exception { get; set; }

        public string TransactionId { get; set; }
    }
}