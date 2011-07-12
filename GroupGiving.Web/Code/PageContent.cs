using System.Collections.Generic;

namespace GroupGiving.Web.Code
{
    public class PageContent : Dictionary<string,string>
    {
        public PageContent() : base()
        {
            
        }
        public PageContent(IEnumerable<KeyValuePair<string,string>> values)
        {
            foreach(var value in values)
                Add(value.Key, value.Value);
        }
    }
}