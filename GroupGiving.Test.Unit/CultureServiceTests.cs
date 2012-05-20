using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Web.Code;
using NUnit.Framework;

namespace GroupGiving.Test.Unit
{
    [TestFixture]
    public class CultureServiceTests
    {
        private ICultureService _cultureService;
        public CultureServiceTests()
        {
            _cultureService = new CultureService();
        }

        [Test]
        public void GivenASetOfPreferredLanguages_TheCorrectPreferredLanguageIsSelected()
        {
            string[] languages = new string[]
                                     {
                                         "en-GB","en-US;q=0.8","en;q=0.6"
                                     };

            string preferred = _cultureService.DeterminePreferredCulture(languages);

            Assert.That(preferred, Is.StringMatching("en-GB"));
        }
    }
}
