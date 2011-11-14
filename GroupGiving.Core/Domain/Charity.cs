using System.Collections.Generic;

namespace GroupGiving.Core.Domain
{
    public class Charity
    {
        public string Id { get; set; }
        public string DonationGatewayName { get; set; }
        public int DonationGatewayCharityId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string RegistrationNumber { get; set; }

        public string LogoUrl { get; set; }

        public string DonationPageUrl { get; set; }
    }
}