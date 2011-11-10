using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmailProcessing;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using Raven.Client;

namespace GroupGiving.Core.Services
{
    public class EventService : IEventService
    {
        private readonly IDocumentStore _documentStore;

        public EventService(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public IEnumerable<GroupGivingEvent> RetrieveAllEvents()
        {
            using (var session = _documentStore.OpenSession())
            {
                return session.Query<GroupGivingEvent>().Take(1024);
            }
        }

        public CreateEventResult CreateEvent(CreateEventRequest request)
        {
            using (var session = _documentStore.OpenSession())
            {
                if (session.Query<GroupGivingEvent>().Any(e=>e.ShortUrl == request.ShortUrl))
                {
                    throw new InvalidOperationException("Short url is not available");
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
                                                   State = EventState.Creating,
                                                   Latitude = request.Latitude,
                                                   Longitude = request.Longitude,
                                                   Postcode = request.Postcode,
                                                   Country = request.Country,
                                                   
                                               };

                session.Store(ggEvent);
                session.SaveChanges();

                int eventId = int.Parse(ggEvent.Id.Substring(ggEvent.Id.IndexOf('/') + 1));
                return new CreateEventResult() { Success = true, Event = ggEvent };
            }

        }

        public void SetTicketDetails(SetTicketDetailsRequest setTicketDetailsRequest)
        {
            using (var session = _documentStore.OpenSession())
            {
                var @event = GetEventByShortUrl(setTicketDetailsRequest.ShortUrl, session);

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
                @event.State = EventState.SalesReady;

                session.SaveChanges();
            }
        }

        private static GroupGivingEvent GetEventByShortUrl(string shortUrl,
                                                           IDocumentSession session)
        {
            var @event =
                session.Query<GroupGivingEvent>().Where(e => e.ShortUrl == shortUrl)
                    .FirstOrDefault();
            return @event;
        }

        public bool ShortUrlAvailable(string shortUrl)
        {
            using (var session = _documentStore.OpenSession())
            {
                return !session.Query<GroupGivingEvent>().Any(e => e.ShortUrl == shortUrl);
            }
        }

        public GroupGivingEvent Retrieve(int eventId)
        {
            using (var session = _documentStore.OpenSession())
            {
                return session.Load<GroupGivingEvent>(eventId);
            }
        }

        public GroupGivingEvent Retrieve(string shortUrl)
        {
            using (var session = _documentStore.OpenSession())
            {
                var @event = session.Query<GroupGivingEvent>().Where(e => e.ShortUrl == shortUrl).FirstOrDefault();
                return @event;
            }
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
