using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Routing;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using System.Web.Mvc.Html;
using Ninject;

namespace GroupGiving.Web.Code
{
    public static class HtmlHelperExtensions
    {
        #region Private Statics

        private static int _seed;
        private static string[] dictionary = "lorem ipsum dolor sit amet consectetuer adipiscing elit sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat ut wisi enim ad minim veniam quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi Ut wisi enim ad minim veniam quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi".Split(' ');

        #endregion

        #region Extension Methods

        [Inject]
        public static IContentProvider ContentProvider;

        public static string FakeLatinParagraph(this HtmlHelper helper)
        {
            return FakeLatinParagraph(helper, dictionary.Length);
        }

        public static string FakeLatinParagraph(this HtmlHelper helper, int wordCount)
        {
            StringBuilder words = new StringBuilder();
            Random rand = null;
            string paragraph = string.Empty;

            if (HtmlHelperExtensions._seed >= int.MaxValue - 1000)
                HtmlHelperExtensions._seed = 0;

            rand = new Random(HtmlHelperExtensions._seed += DateTime.Now.Millisecond);

            for (int n = 0; n < wordCount; n++)
                words.AppendFormat("{0} ", dictionary[rand.Next(0, dictionary.Length)]);

            paragraph = words.ToString();
            paragraph = string.Format("{0}{1}.", char.ToUpperInvariant(paragraph[0]), paragraph.Substring(1).TrimEnd());

            return paragraph;
        }

        public static string FakeLatinParagraphs(this HtmlHelper helper, int paragraphCount, int wordsPerParagraph, string beforeParagraph, string afterParagraph)
        {
            StringBuilder paragraphs = new StringBuilder();

            for (int n = 0; n < paragraphCount; n++)
            {
                paragraphs.AppendFormat("\n{0}\n", beforeParagraph);
                paragraphs.AppendFormat("\t{0}", HtmlHelperExtensions.FakeLatinParagraph(helper, wordsPerParagraph));
                paragraphs.AppendFormat("\n{0}\n", afterParagraph);
            }

            return paragraphs.ToString();
        }

        public static string FakeLatinTitle(this HtmlHelper helper, int wordCount)
        {
            string title = HtmlHelperExtensions.FakeLatinParagraph(helper, wordCount);

            title = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(title);
            title = title.Substring(0, title.Length - 1); // kill period from paragraph
            return title;
        }

        #endregion

        public static MvcHtmlString Content(this HtmlHelper html, string defaultContent = "", string pageAddress = "", string label = "")
        {
            string culture = html.ViewContext.RequestContext.HttpContext.Request.Cookies["culture"] != null
                                 ? html.ViewContext.RequestContext.HttpContext.Request.Cookies["culture"].Value
                                 : "en";

            if (string.IsNullOrWhiteSpace(label))
            {
                if (string.IsNullOrWhiteSpace(defaultContent))
                {
                    throw new ArgumentException("If no label is provided then a default content must be provided");
                }
                label = HttpUtility.UrlEncode(defaultContent);
            }

            if (string.IsNullOrWhiteSpace(pageAddress))
            {
                var view = html.ViewContext.View as RazorView;
                if (view != null)
                {
                    pageAddress = view.ViewPath;
                }
                else
                {
                    pageAddress = html.ViewContext.RequestContext.HttpContext.Request.Url.AbsolutePath;
                }
            }

            var pageContent = PageContentService.Provider.GetPage(pageAddress);
            if (pageContent == null)
            {
                pageContent = PageContentService.Provider.AddContentPage(pageAddress);
            }
            var contentDefinition = pageContent.Content.Where(cd => cd.Label == label).FirstOrDefault();
            if (contentDefinition == null)
            {
                contentDefinition = PageContentService.Provider.AddContentDefinition(pageContent, label, defaultContent, culture);
            }

            string content = "";
            if (contentDefinition.ContentByCulture.ContainsKey(culture))
                content = contentDefinition.ContentByCulture[culture] ?? "";
            else
            {
                if (contentDefinition.ContentByCulture.Count > 0)
                    content = contentDefinition.ContentByCulture.ElementAt(0).Value;
            }

            // editing options
            var editmode = html.ViewContext.RequestContext.HttpContext.Request.QueryString["editmode"];
            if (!string.IsNullOrWhiteSpace(editmode))
            {
                string beginTag = string.Format("<span page-id='{0}' label='{1}' culture='{2}'>", 
                                                pageContent.Id.Substring(pageContent.Id.IndexOf('/')+1),
                                                contentDefinition.Label, 
                                                culture);
                string endTag = "</span>";
                content = string.Concat(beginTag, content, endTag);
            }


            return new MvcHtmlString(content);
        }

        public static MvcHtmlString HintFor<TModel,TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var display = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            return MvcHtmlString.Create(display.Description);
        }

        public static string CurrentCulture(this HtmlHelper html)
        {
            return html.ViewContext.RequestContext.HttpContext.Request.Cookies["culture"] != null
                                 ? html.ViewContext.RequestContext.HttpContext.Request.Cookies["culture"].Value
                                 : "en";
        }

        public static DateTimeFormatInfo CultureDateTimeFormat(this HtmlHelper html)
        {
            return html.CultureDateTimeFormat(html.CurrentCulture());
        }
        public static DateTimeFormatInfo CultureDateTimeFormat(this HtmlHelper html, string culture)
        {
            var dtfi = DateTimeFormatInfo.GetInstance(new CultureInfo(culture));
            switch(culture)
            {
                case "pl":
                    dtfi.DayNames = new string[]
                                        {
                                            "niedziela",
                                            "poniedziałek",
                                            "wtorek",
                                            "środa",
                                            "czwartek",
                                            "piątek",
                                            "sobota"
                                        };
                    dtfi.MonthNames = dtfi.MonthGenitiveNames = new string[]
                                                                    {
                                                                        "styczeń",
                                                                        "luty",
                                                                        "marzec",
                                                                        "kwiecień",
                                                                        "maj",
                                                                        "czewriec",
                                                                        "lipiec",
                                                                        "sierpień",
                                                                        "wrzesień",
                                                                        "październik",
                                                                        "listopad",
                                                                        "grudzień",
                                                                        ""
                                                                    };
                    return dtfi;
                default:
                    return new DateTimeFormatInfo();
            }
        }

        public static CultureInfo CurrencyFormat(this HtmlHelper html, string currency)
        {
            switch (currency)
            {
                case "PLN":
                    return CultureInfo.GetCultureInfo("pl-PL");
                case "USD":
                    return CultureInfo.GetCultureInfo("en-US");
                default:
                    return CultureInfo.GetCultureInfo("en-GB");
            }
        }

        public static string ChangeCultureForUri(this HtmlHelper html, Uri uri, string newCulture)
        {
            var currentRoute = RouteUtils.GetRouteDataByUrl("/" + uri.PathAndQuery);

            if (currentRoute==null)
            {
                return "/" + newCulture;
            }

            if (currentRoute.Values.ContainsKey("culture"))
            {
                if (newCulture != "en")
                    currentRoute.Values["culture"] = newCulture;
                else
                    currentRoute.Values.Remove("culture");
            } else if (newCulture != "en")
            {
                currentRoute.Values.Add("culture", newCulture);
            }

            string link = RouteTable.Routes.GetVirtualPath(html.ViewContext.RequestContext, currentRoute.Values).VirtualPath;

            return link;
        }
    }

    public static class RouteUtils
    {
        public static RouteData GetRouteDataByUrl(string url)
        {
            return RouteTable.Routes.GetRouteData(new RewritedHttpContextBase(url));
        }

        private class RewritedHttpContextBase : HttpContextBase
        {
            private readonly HttpRequestBase mockHttpRequestBase;

            public RewritedHttpContextBase(string appRelativeUrl)
            {
                this.mockHttpRequestBase = new MockHttpRequestBase(appRelativeUrl);
            }


            public override HttpRequestBase Request
            {
                get
                {
                    return mockHttpRequestBase;
                }
            }

            private class MockHttpRequestBase : HttpRequestBase
            {
                private readonly string appRelativeUrl;

                public MockHttpRequestBase(string appRelativeUrl)
                {
                    this.appRelativeUrl = appRelativeUrl;
                }

                public override string AppRelativeCurrentExecutionFilePath
                {
                    get { return appRelativeUrl; }
                }

                public override string PathInfo
                {
                    get { return ""; }
                }
            }
        }
    }
}