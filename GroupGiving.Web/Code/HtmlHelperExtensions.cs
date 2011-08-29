using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Text;
using GroupGiving.Core.Data;
using GroupGiving.Core.Services;

namespace GroupGiving.Web.Code
{
    public static class HtmlHelperExtensions
    {
        #region Private Statics

        private static int _seed;
        private static string[] dictionary = "lorem ipsum dolor sit amet consectetuer adipiscing elit sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat ut wisi enim ad minim veniam quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi Ut wisi enim ad minim veniam quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi".Split(' ');

        #endregion

        #region Extension Methods

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

        public static string Content(this HtmlHelper helper, string key)
        {
            var contentId = key.Split('-');
            return MvcApplication.PageContent.Get("en-GB", contentId[0], contentId[1]);
        }

        public static string Content(this HtmlHelper helper, string page, string key)
        {
            return MvcApplication.PageContent.Get("en-GB", page, key);
        }

        #endregion

        public static MvcHtmlString HintFor<TModel,TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var display = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            return MvcHtmlString.Create(display.Description);
        }
    }
}