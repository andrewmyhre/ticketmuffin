using GroupGiving.Core.Dto;

namespace GroupGiving.Core.Actions.CreatePledge
{
    public class CreatePledgeActionResult
    {
        public IPaymentGatewayResponse GatewayResponse { get; set; }

        public bool Succeeded { get; set; }
    }
}