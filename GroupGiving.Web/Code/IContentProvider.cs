using GroupGiving.Core.Domain;

namespace GroupGiving.Web.Code
{
    public interface IContentProvider
    {
        void Initialise();
        //string GetContent(string pageAddress, string contentLabel, string culture);
        PageContent GetPage(string address);
        PageContent AddContentPage(string pageAddress);
        ContentDefinition AddContentDefinition(PageContent pageContent, string label, string defaultContent, string culture);
    }
}