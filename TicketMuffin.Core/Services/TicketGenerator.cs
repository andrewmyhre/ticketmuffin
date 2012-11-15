using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Raven.Client;
using TicketMuffin.Core.Domain;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace TicketMuffin.Core.Services
{
    public class TicketGenerator : ITicketGenerator
    {
        private readonly IDocumentSession _ravenSession;
        private readonly string _ticketTemplatePath;
        static string ticketFolder = "c:\\TicketMuffinTickets";

        public TicketGenerator(IDocumentSession ravenSession, string ticketTemplatePath)
        {
            _ravenSession = ravenSession;
            _ticketTemplatePath = ticketTemplatePath;
        }

        public void CreateTicket(GroupGivingEvent @event, EventPledge pledge, EventPledgeAttendee attendee, string culture)
        {
            string ticketFilename = "";
            if (attendee.TicketNumber == null)
            {
                attendee.TicketNumber = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "");
                _ravenSession.SaveChanges();
            }
            ticketFilename = string.Format("{0}.pdf", attendee.TicketNumber);
            var pdfPath = Path.Combine(ticketFolder, ticketFilename);
            if (!Directory.Exists(Path.GetDirectoryName(pdfPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(pdfPath));

            var cultureInfo = new CultureInfo(culture);

            using (var outputStream = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
            {
                PdfReader reader = new PdfReader(_ticketTemplatePath);


                Document document = new Document(new Rectangle(7.48f * 72, 3.15f * 72));
                var writer = PdfWriter.GetInstance(document, outputStream);
                document.Open();
                PdfContentByte cb = writer.DirectContent;
                PdfImportedPage page = writer.GetImportedPage(reader, 1);
                cb.SetFontAndSize(FontFactory.GetFont(FontFactory.HELVETICA).BaseFont, 12);
                cb.AddTemplate(page, 0, 0);
                cb.BeginText();
                AddTextToDocument(80, 190, 280, 30, @event.Title, cb);
                AddTextToDocument(80, 130, 280, 30, @event.StartDate.ToString(cultureInfo), cb);
                //AddTextToDocument(80, 80, 280, 30, @event.Venue, cb);
                AddTextToDocument(80, 80, 280, 30, string.Join(", ", @event.Venue, @event.AddressLine, @event.City + " " + @event.Postcode, @event.Country), cb);
                AddTextToDocument(80, 15, 280, 30, @event.OrganiserName, cb);
                AddTextToDocument(380, 140, 100, 30, attendee.FullName, cb);
                AddTextToDocument(380, 126, 100, 30, attendee.TicketNumber, cb);
                AddTextToDocument(380, 80, 100, 30, pledge.Total.ToString("c", cultureInfo), cb);
                AddTextToDocument(380, 35, 100, 30, pledge.AccountName, cb);
                AddTextToDocument(380, 21, 100, 30, pledge.AccountEmailAddress, cb);
                AddTextToDocument(380, 7, 100, 30, pledge.OrderNumber, cb);

                cb.EndText();
                document.Close();
            }
            
        }

        public Stream LoadTicket(GroupGivingEvent @event, EventPledge pledge, EventPledgeAttendee attendee, string culture)
        {
            string ticketFilename = "";
            
            if (attendee.TicketNumber==null)
            {
                CreateTicket(@event,pledge,attendee,culture);
            }
            
            ticketFilename = string.Format("{0}.pdf", attendee.TicketNumber);

            if (!File.Exists(Path.Combine(ticketFolder, ticketFilename)))
            {
                CreateTicket(@event, pledge, attendee, culture);
            }

            return File.OpenRead(Path.Combine(ticketFolder, ticketFilename));
        }

        private static void AddTextToDocument(int left, int top, float width, float height, string text, PdfContentByte cb)
        {
            int leading = 12;
            ColumnText ct = new ColumnText(cb);
            ct.SetSimpleColumn(
                new Phrase(new Chunk(text, FontFactory.GetFont(FontFactory.HELVETICA,12,0))),
                left,
                top + leading,
                left + width,
                top - height + leading, 
                leading, 
                Element.ALIGN_LEFT | Element.ALIGN_TOP);
            //cb.SetTextMatrix(left, top);
            //cb.ShowText(text);
            ct.Go();
        }
    }
}