using System;
using TicketMuffin.Core.Services;

namespace TicketMuffin.Web.Code
{
    [Obsolete("Static classes are gay", true)]
    public class PageContentService
    {
        private static IContentProvider _provider=null;
        public static IContentProvider Provider
        {
            get
            {
                return _provider;
            }
        }
       
    }
}