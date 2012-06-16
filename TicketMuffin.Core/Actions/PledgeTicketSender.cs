using System.Net.Mail;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;

namespace TicketMuffin.Core.Actions
{
    public class PledgeTicketSender : IPledgeTicketSender
    {
        private readonly ITicketGenerator _ticketGenerator;
        private readonly IEventCultureResolver _eventCultureResolver;

        public PledgeTicketSender(ITicketGenerator ticketGenerator, IEventCultureResolver eventCultureResolver)
        {
            _ticketGenerator = ticketGenerator;
            _eventCultureResolver = eventCultureResolver;
        }

        public void SendTickets(GroupGivingEvent @event, EventPledge pledge)
        {
            MailMessage message = new MailMessage("sales@ticketmuffin.com",
                                                  pledge.AccountEmailAddress);
            message.Subject = "Your tickets for " + @event.Title;
            message.Body = @"Hi " + pledge.AccountName + "," +
                           "Your tickets for " + @event.Title + " are attached." +
                           "" +
                           "Thanks for using TicketMuffin!";
            message.IsBodyHtml = false;
            foreach (var attendee in pledge.Attendees)
            {
                var ticket = _ticketGenerator.LoadTicket(@event, pledge, attendee, _eventCultureResolver.ResolveCulture(@event));

                message.Attachments.Add(new Attachment(ticket, "ticket.pdf", "application/pdf"));
            }

            var smtp = new SmtpClient();
            smtp.Send(message);
        }
    }
}