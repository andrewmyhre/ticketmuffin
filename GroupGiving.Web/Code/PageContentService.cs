using System.Collections.Generic;

namespace GroupGiving.Web.Code
{
    public class PageContentService
    {
        private static IContentProvider _provider=null;
        public static IContentProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    _provider = new RavenDbContentProvider(RavenDbDocumentStore.Instance);
                }

                return _provider;
            }
        }
       
    }
}