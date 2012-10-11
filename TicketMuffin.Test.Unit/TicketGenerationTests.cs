using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using NUnit.Mocks;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;

namespace GroupGiving.Test.Unit.Pledging
{
    [TestFixture]
    public class TicketGenerationTests
    {
        [Test]
        public void GenerateATicket()
        {
            var @event = new GroupGivingEvent()
                {
                    Title = "this is a test",
                    AddressLine = "123 my street",
                    City="London",
                    Country = "United Kingdom",
                    Postcode = "E2 0BY",
                    Pledges = new List<EventPledge>()
                        {
                            new EventPledge()
                                {
                                    AccountEmailAddress = "test@test.com",
                                    AccountName = "test account user",
                                    Attendees = new List<EventPledgeAttendee>()
                                        {
                                            new EventPledgeAttendee("chris"){TicketNumber="00001"},
                                            new EventPledgeAttendee("dave"){TicketNumber="00002"},
                                            new EventPledgeAttendee("john"){TicketNumber="00003"}
                                        },
                                        OrderNumber = "12345",
                                        Paid = true,

                                },
                        },
                        StartDate = DateTime.Now.AddDays(7),
                        OrganiserName = "event organiser",
                        Venue = "my house"
                };


            var ravenSessionMock = new Mock<IDocumentSession>();
            var ticketGenerator = new TicketGenerator(ravenSessionMock.Object, "tickets\\ticket-pl.pdf");
            ticketGenerator.CreateTicket(@event, @event.Pledges.First(), @event.Pledges.First().Attendees.First(), "en-GB");
        }
    }
}