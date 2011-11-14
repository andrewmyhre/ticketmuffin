using System;
using GroupGiving.PayPal.AdaptiveAccounts;
using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;
using PayPal.Platform.SDK;
using PayPal.Services.Private.AA;


namespace GroupGiving.PayPal
{
    public class PaypalAccountService : IPaypalAccountService
    {
        private readonly PaypalAdaptiveAccountsConfigurationSection _configuration;

        public PaypalAccountService(PaypalAdaptiveAccountsConfigurationSection configuration)
        {
            _configuration = configuration;
        }

        public VerifyPaypalAccountResponse VerifyPaypalAccount(VerifyPaypalAccountRequest request)
        {
            BaseAPIProfile profile = BaseApiProfileFactory.CreateFromConfiguration(_configuration);

            GetVerifiedStatusRequest getVerifiedStatusRequest = new GetVerifiedStatusRequest();
            getVerifiedStatusRequest.emailAddress = request.Email;
            getVerifiedStatusRequest.firstName = request.FirstName;
            getVerifiedStatusRequest.lastName = request.LastName;
            getVerifiedStatusRequest.matchCriteria = "NAME";

            AdaptiveAccountsSdk aa = new AdaptiveAccountsSdk();
            aa.APIProfile = profile;
            var response = new VerifyPaypalAccountResponse();
            try
            {
                var verifyResponse = aa.GetVerifiedStatus(getVerifiedStatusRequest);

                if (verifyResponse == null)
                {
                    response.Success = false;
                    response.AccountStatus = "";
                    return response;
                }
                else if (verifyResponse.responseEnvelope.ack != AckCode.Success)
                {
                    response.Success = false;
                    response.AccountStatus = "";
                    return response;
                }

                response.Success = true;
                response.AccountStatus = verifyResponse.accountStatus;
                return response;
            }
            catch
            {
                response.Success = false;
                response.AccountStatus = "";
                return response;
            }
        }

        public CreatePaypalAccountResponse CreateAccount(CreatePayPalAccountRequest request)
        {
            CreateAccountRequest paypalRequest = new CreateAccountRequest();

            paypalRequest.accountType = "BUSINESS";
            paypalRequest.name = new NameType();
            paypalRequest.name.salutation = request.Salutation;
            paypalRequest.name.firstName = request.FirstName;
            paypalRequest.name.middleName = "";
            paypalRequest.name.lastName = request.LastName;
            paypalRequest.dateOfBirth = request.DateOfBirth;
            paypalRequest.address = new AddressType();
            paypalRequest.address.line1 = request.AddressLine1;
            paypalRequest.address.line2 = request.AddressLine2;
            paypalRequest.address.city = request.City;
            paypalRequest.address.state = request.State;
            paypalRequest.address.postalCode = request.PostCode;
            paypalRequest.address.countryCode = request.CountryCode;
            paypalRequest.citizenshipCountryCode = request.CountryCode;
            paypalRequest.partnerField1 = "";
            paypalRequest.partnerField2 = "";
            paypalRequest.partnerField3 = "";
            paypalRequest.partnerField4 = "";
            paypalRequest.partnerField5 = "";
            paypalRequest.currencyCode = request.CurrencyCode;
            paypalRequest.contactPhoneNumber = request.ContactPhoneNumber;
            paypalRequest.preferredLanguageCode = "en_GB";
            paypalRequest.clientDetails = new ClientDetailsType();
            paypalRequest.clientDetails.applicationId = _configuration.ApplicationId;
            paypalRequest.clientDetails.deviceId = _configuration.DeviceIpAddress.Replace(".","");
            paypalRequest.clientDetails.ipAddress = _configuration.DeviceIpAddress;
            paypalRequest.emailAddress = "ticketmuffin." + Guid.NewGuid().ToString() + "@gmail.com";
            //AccountRequest.sandboxEmailAddress = "platform.sdk.seller@gmail.com";
            paypalRequest.createAccountWebOptions = new CreateAccountWebOptionsType();
            paypalRequest.createAccountWebOptions.returnUrl = request.ReturnUrl;
            paypalRequest.registrationType = "WEB";


            ////Business Info
            paypalRequest.businessInfo = new BusinessInfoType();
            paypalRequest.businessInfo.businessName = string.Format("{0} {1}", request.FirstName, request.LastName);
            paypalRequest.businessInfo.businessAddress = new AddressType();
            paypalRequest.businessInfo.businessAddress.line1 = request.AddressLine1;
            paypalRequest.businessInfo.businessAddress.line2 = request.AddressLine2;
            paypalRequest.businessInfo.businessAddress.city = request.City;
            paypalRequest.businessInfo.businessAddress.state = request.State;
            paypalRequest.businessInfo.businessAddress.postalCode = request.PostCode;
            paypalRequest.businessInfo.businessAddress.countryCode = request.CountryCode;
            paypalRequest.businessInfo.workPhone = request.ContactPhoneNumber;
            paypalRequest.businessInfo.merchantCategoryCode = "8398";//Charitable and Social Service Organizations - Fundraising
            //paypalRequest.businessInfo.category = "8398"; //Charitable and Social Service Organizations - Fundraising
            //paypalRequest.businessInfo.subCategory = "2001";
            paypalRequest.businessInfo.customerServiceEmail = request.ContactEmailAddress;
            paypalRequest.businessInfo.customerServicePhone = request.ContactPhoneNumber;
            paypalRequest.businessInfo.webSite = request.MerchantWebsiteAddress; // make this ticketmuffin
            
            // date of establishment only required for the following countries
            switch(request.CountryCode)
            {
                case "US":
                case "UK":
                case "CA":
                case "DE":
                case "ES":
                case "IT":
                case "CZ":
                case "SE":
                case "DK":
                    paypalRequest.businessInfo.dateOfEstablishment = request.OrganisationDateOfEstablisment;
                    paypalRequest.businessInfo.dateOfEstablishmentSpecified = true;
                    break;
            }

            paypalRequest.businessInfo.businessType = BusinessType.INDIVIDUAL;
            paypalRequest.businessInfo.businessTypeSpecified = true;
            paypalRequest.businessInfo.averagePrice = Convert.ToDecimal("1.00");
            paypalRequest.businessInfo.averagePriceSpecified = true;
            paypalRequest.businessInfo.averageMonthlyVolume = Convert.ToDecimal("100");
            paypalRequest.businessInfo.averageMonthlyVolumeSpecified = true;
            paypalRequest.businessInfo.percentageRevenueFromOnline = "60";
            paypalRequest.businessInfo.salesVenue = new SalesVenueType[1];
            paypalRequest.businessInfo.salesVenue[0] = new SalesVenueType();
            paypalRequest.businessInfo.salesVenue[0] = SalesVenueType.WEB;
            ////

            BaseAPIProfile profile = BaseApiProfileFactory.CreateFromConfiguration(_configuration);

            AdaptiveAccountsSdk aa = new AdaptiveAccountsSdk();
            profile.SandboxMailAddress = "andrew.myhre@gmail.com";
            aa.APIProfile = profile;
            var paypalResponse = aa.CreateAccount(paypalRequest);

            CreatePaypalAccountResponse response = new CreatePaypalAccountResponse();
            response.Success = paypalResponse.responseEnvelope.ack == AckCode.Success;
            response.Id = paypalResponse.accountId;
            response.RedirectUrl = paypalResponse.redirectURL;
            response.CreateAccountKey = paypalResponse.createAccountKey;
            return response;
        }
    }
}