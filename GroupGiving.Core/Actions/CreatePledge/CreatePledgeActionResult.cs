using System;
using GroupGiving.PayPal.Model;

namespace GroupGiving.Core.Actions.CreatePledge
{
    public class CreatePledgeActionResult
    {
        public PaymentGatewayResponse GatewayResponse { get; set; }

        public bool Succeeded { get; set; }
    }
}