using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using GroupGiving.Core.Domain;

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

    public class ContentDictionaries : Dictionary<string,PageContent>
    {
    }

    public class XmlContentProvider : IContentProvider
    {
        CultureInfo[] allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        ContentDictionaries _dictionaries = new ContentDictionaries();

        public void Initialise(string contentFolderPath)
        {
            var mainFolder = new DirectoryInfo(contentFolderPath);
            if (!mainFolder.Exists)
                throw new InvalidOperationException(string.Format("Could not locate {0}", contentFolderPath));
            var langFolders = mainFolder.GetDirectories();
            foreach(var folder in langFolders)
            {
                string name = folder.Name;
                if (!IsValidLocale(name))
                    continue;

                var files = folder.GetFiles("content.xml");
                if (files.Length==0)
                    throw new InvalidOperationException(string.Format("Could not find content file in {0}", folder.FullName));

                var contentFilePath = files.First().FullName;
                LoadContentFromXml(contentFilePath);
            }
        }

        public string Get(string locale, string page, string label)
        {
            return _dictionaries[locale][string.Format("{0}-{1}", page, label)];
        }

        private void LoadContentFromXml(string contentFilePath)
        {
            XDocument xml = XDocument.Load(contentFilePath);
            var elements = xml.Element("contentDefinitions").Elements("content");
            string locale = xml.Element("contentDefinitions").Attribute("locale").Value;
            _dictionaries.Add(locale,
                new PageContent(
                    (from c in elements select new KeyValuePair<string, string>(
                        string.Format("{0}-{1}", c.Attribute("page").Value, c.Attribute("label").Value),
                        c.Value))
                    ));


        }


        private bool IsValidLocale(string localeName)
        {
            return (allCultures.Any(c => c.Name == localeName));
        }
    }

    public interface IContentProvider
    {
        void Initialise(string contentFolderPath);
    }
}