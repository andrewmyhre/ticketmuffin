using System;
using System.Net.Mail;
using GroupGiving.Core.Domain;

namespace GroupGiving.Core.Email
{
    public class MinimumAttendeesReached : IEmailTemplate
    {
        private readonly GroupGivingEvent _event;
        private readonly EventPledge _pledge;

        private static EmailTemplate _template = new EmailTemplate()
        {
            Body="The minimum number of attendees has been reached",
            FromAddress="noreply@ticketmuffin.com",
            FromName="TicketMuffin",
            IsHtml=false,
            SenderAddress="noreply@ticketmuffin.com",
            SenderName="TicketMuffin",
            Subject="@Model.Event.Title has reached the minimum pledges"
        };

        public MinimumAttendeesReached(GroupGivingEvent @event, EventPledge pledge)
        {
            _event = @event;
            _pledge = pledge;
        }

        public MailMessage ToMailMessage()
        {
            MailMessage message = EmailTemplate.ToMailMessage(_template);
            message.To.Add(new MailAddress(_pledge.AccountEmailAddress));

            return message;
        }
    }
}