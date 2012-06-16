using System;
using System.Collections.Generic;
using System.Linq;
using EmailProcessing;
using Raven.Client;
using Raven.Client.Linq;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public class EventService : IEventService
    {
        private readonly IDocumentSession _ravenSession;

        public EventService(IDocumentSession ravenSession)
        {
            _ravenSession = ravenSession;
        }

        public IEnumerable<GroupGivingEvent> List(int pageSize=20, int pageIndex=0)
        {
            return Queryable.Skip(_ravenSession.Query<GroupGivingEvent>(), pageIndex*pageSize).Take(pageSize);
        }

        public CreateEventResult CreateEvent(CreateEventRequest request)
        {
                if (_ravenSession
                    .Query<GroupGivingEvent>()
                    .Any(e=>e.ShortUrl == request.ShortUrl && e.State != EventState.Deleted))
                {
                    throw new InvalidOperationException("Short url is not available");
                }

                Charity charity = null;
                if (request.ForCharity)
                {
                    if (!request.CharityId.HasValue)
                    {
                        throw new ArgumentException("Charity Id must be provided if event is for a charity");
                    }

                    charity = _ravenSession.Query<Charity>()
                        .FirstOrDefault(c => c.DonationGatewayName == request.CharityDonationGatewayName
                                             && c.DonationGatewayCharityId == request.CharityId);

                    if (charity == null)
                    {
                        charity = new Charity()
                                      {
                                          Name = request.CharityName,
                                          Description = request.CharityDescription,
                                          RegistrationNumber = request.CharityRegistrationNumber,
                                          DonationGatewayName = request.CharityDonationGatewayName,
                                          DonationGatewayCharityId = request.CharityId.Value,
                                          LogoUrl = request.CharityLogoUrl,
                                          DonationPageUrl = request.CharityDonationPageUrl
                                      };
                        _ravenSession.Store(charity);
                    }
                }

            GroupGivingEvent ggEvent = new GroupGivingEvent()
                {
                    Title = request.Title,
                    Description = request.Description,
                    City = request.City,
                    StartDate = request.StartDateTime,
                    Venue = request.Venue,
                    AddressLine = request.AddressLine,
                    ShortUrl = request.ShortUrl,
                    IsPrivate = request.IsPrivate,
                    IsFeatured = request.IsFeatured,
                    PhoneNumber = request.PhoneNumber,
                    OrganiserId = request.OrganiserAccountId,
                    OrganiserName = request.OrganiserName,
                    State = EventState.Creating,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Postcode = request.Postcode,
                    Country = request.Country,
                                                   
                    ImageFilename=request.ImageFilename,
                    ImageUrl=request.ImageUrl,
                    CharityDetails = charity
                };


            _ravenSession.Store(ggEvent);

            return new CreateEventResult() { Success = true, Event = ggEvent };
        }

        public void SetTicketDetails(SetTicketDetailsRequest setTicketDetailsRequest)
        {
            var @event = GetEventByShortUrl(setTicketDetailsRequest.ShortUrl, _ravenSession);

            if (@event == null)
                throw new ArgumentException("Event not found");

            @event.TicketPrice = setTicketDetailsRequest.TicketPrice.Value;
            @event.MinimumParticipants = setTicketDetailsRequest.MinimumParticipants.Value;
            @event.MaximumParticipants = setTicketDetailsRequest.MaximumParticipants;
            @event.SalesEndDateTime = setTicketDetailsRequest.SalesEndDateTime;
            @event.AdditionalBenefits = setTicketDetailsRequest.AdditionalBenefits;
            @event.PaypalAccountEmailAddress = setTicketDetailsRequest.PayPalEmail;
            @event.PayPalAccountFirstName = setTicketDetailsRequest.PayPalFirstName;
            @event.PayPalAccountLastName = setTicketDetailsRequest.PayPalLastName;
            @event.Currency = (int) setTicketDetailsRequest.Currency;
            @event.State = EventState.SalesReady;

            _ravenSession.SaveChanges();
        }

        private static GroupGivingEvent GetEventByShortUrl(string shortUrl,
                                                           IDocumentSession session)
        {
            return session.Query<GroupGivingEvent>().SingleOrDefault(e => e.ShortUrl == shortUrl);
        }

        public bool ShortUrlAvailable(string shortUrl)
        {
            return !_ravenSession.Query<GroupGivingEvent>()
                .Any(e => e.ShortUrl == shortUrl && e.State != EventState.Deleted);
        }

        public GroupGivingEvent Retrieve(int eventId)
        {
            return _ravenSession.Load<GroupGivingEvent>(eventId);
        }

        public GroupGivingEvent Retrieve(string shortUrl)
        {
            RavenQueryStatistics stats;
            var @event = _ravenSession.Query<GroupGivingEvent>()
                .Statistics(out stats)
                .Customize(a=>a.WaitForNonStaleResults(TimeSpan.FromSeconds(5)))
                .SingleOrDefault(e => e.ShortUrl == shortUrl && e.State != EventState.Deleted);
            return @event;
        }

        public void SendEventInvitationEmails(IEmailPackageRelayer emailPackageRelayer, string recipients, string body, string subject)
        {
            EmailPackage package = new EmailPackage();
            package.Subject = subject;
            package.Text = body;
            package.From = "noreply@ticketmuffin.com";

            string[] recipientList = recipients.Split(',');
            foreach (var recipient in recipientList)
            {
                package.To.Add(recipient.Trim());
            }
            emailPackageRelayer.Relay(package);
        }

    }
}
