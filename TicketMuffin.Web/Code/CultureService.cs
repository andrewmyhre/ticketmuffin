using System;
using System.Linq;
using System.Web;

namespace TicketMuffin.Web.Code
{
    public class CultureService : ICultureService
    {
        public string GetCultureOrDefault(HttpContextBase httpContext, string defaultCulture="en-GB")
        {
            var culture= httpContext.Request.Cookies["culture"];
            if (culture!=null)
                return culture.Value;

            if (httpContext.Items["culture"] != null)
                return (string)httpContext.Items["culture"];

            return defaultCulture;
        }

        public string DeterminePreferredCulture(string[] preferredLanguages)
        {
            if (preferredLanguages == null||preferredLanguages.Length == 0)
                return "en-GB";

            if (preferredLanguages.Length == 1)
            {
                return ParseCultureWeightSetting(preferredLanguages[0]).Culture;
            }

            var cultureWeights = preferredLanguages.Select(lang=>ParseCultureWeightSetting(lang));
            if (cultureWeights.Any())
            {
                return cultureWeights.OrderByDescending(cw => cw.Weight).Select(cw => cw.Culture).FirstOrDefault();
            }

            return "en-GB";
        }

        private CultureWeight ParseCultureWeightSetting(string languageSetting)
        {
            var values1 = languageSetting.Split(new [] {';'}, StringSplitOptions.RemoveEmptyEntries);
            if(values1.Length==0 || values1.Length > 2)
                throw new ArgumentException("languageSetting must be in the format [culture];q=[weight]", languageSetting);

            var cultureWeight = new CultureWeight() {Culture = values1[0]};

            decimal weight = 1;
            if (values1.Length == 2)
            {
                // format should be q=1
                string weightString = values1[1].Substring(values1[1].IndexOf('=') + 1);
                decimal.TryParse(weightString, out weight);
            }
            cultureWeight.Weight = weight;
            return cultureWeight;
        }

        public void SetCurrentCulture(HttpContextBase httpContext, string cultureString)
        {
            httpContext.Response.Cookies.Add(new HttpCookie("culture", cultureString));
            httpContext.Items["culture"] = cultureString;
        }

        public bool HasCulture(HttpContextBase httpContext)
        {
            return httpContext.Request.Cookies["culture"] != null
                || httpContext.Items["culture"] != null;
        }
    }

    public class CultureWeight
    {
        public decimal Weight;
        public string Culture;
    }

    public interface ICultureService
    {
        string GetCultureOrDefault(HttpContextBase httpContext, string defaultCulture="en-GB");
        void SetCurrentCulture(HttpContextBase httpContext, string cultureString);
        bool HasCulture(HttpContextBase httpContext);
        string DeterminePreferredCulture(string[] preferredLanguages);
    }
}