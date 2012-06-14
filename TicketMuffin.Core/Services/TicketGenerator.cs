using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using TicketMuffin.Core.Domain;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace TicketMuffin.Core.Services
{
    public class TicketGenerator : ITicketGenerator
    {
        public void CreatePdfFile(GroupGivingEvent @event, out string outputPath)
        {
            throw new NotImplementedException();
        }

        public Stream CreatePdf(GroupGivingEvent @event, EventPledge pledge, EventPledgeAttendee attendee, string culture)
        {
            byte[] data = null;
            var mapPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/tickets/"+pledge.OrderNumber+"-"+Strip(attendee.FullName)+".pdf");
            if (!Directory.Exists(Path.GetDirectoryName(mapPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(mapPath));

            var cultureInfo= new CultureInfo(culture);
            

            using (var outputStream = new FileStream(mapPath, FileMode.Create, FileAccess.Write))
            {
                PdfReader reader = new PdfReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/tickets/ticket-pl.pdf"));
                

                Document document = new Document(new Rectangle(7.48f*72, 3.15f*72));
                var writer = PdfWriter.GetInstance(document, outputStream);
                document.Open();
                PdfContentByte cb = writer.DirectContent;
                PdfImportedPage page = writer.GetImportedPage(reader, 1);
                cb.SetFontAndSize(FontFactory.GetFont(FontFactory.HELVETICA).BaseFont, 12);
                cb.AddTemplate(page, 0, 0);
                cb.BeginText();
                AddTextToDocument(80, 190, 280, 30, @event.Title, cb);
                AddTextToDocument(80, 130, 280, 30, @event.StartDate.ToString(cultureInfo), cb);
                AddTextToDocument(80, 75, 280, 30, @event.Venue, cb);
                AddTextToDocument(80, 15, 280, 30, @event.OrganiserName, cb);
                AddTextToDocument(380, 140, 100,30, attendee.FullName, cb);
                AddTextToDocument(380, 80, 100, 30, pledge.Total.ToString("c", cultureInfo), cb);
                AddTextToDocument(380, 35,100, 30, pledge.AccountName, cb);

                cb.EndText();
                document.Close();
            }
            return new FileStream(mapPath, FileMode.Open, FileAccess.Read);
        }

        private string Strip(string fullName)
        {
            Regex r = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            return r.Replace(fullName, String.Empty);
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