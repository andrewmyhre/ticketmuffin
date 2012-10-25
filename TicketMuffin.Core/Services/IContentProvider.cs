using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public interface IContentProvider
    {
        void Initialise();
        //string GetContent(string pageAddress, string contentLabel, string culture);
        ContentDefinition AddContentDefinition(PageContent pageContent, string label, string defaultContent, string culture);
        void Flush();
        string GetContent(string pageAddress, string label, string defaultContent, string culture, out PageContent pageContent, out string contentLabel);
    }
}