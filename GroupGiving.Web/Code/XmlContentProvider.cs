using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using GroupGiving.Core.Domain;
using log4net;

namespace GroupGiving.Web.Code
{
    public class XmlContentProvider : IContentProvider
    {
        CultureInfo[] allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        ContentDictionaries _dictionaries;
        private FileSystemWatcher _fileWatcher;
        private ILog log = LogManager.GetLogger("XmlContentProvider");

        public void Initialise(string contentFolderPath)
        {
            var mainFolder = new DirectoryInfo(contentFolderPath);
            if (!mainFolder.Exists)
                throw new InvalidOperationException(string.Format("Could not locate {0}", contentFolderPath));
            
            _dictionaries = new ContentDictionaries();
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

            _fileWatcher = new FileSystemWatcher(contentFolderPath, "*.xml");
            _fileWatcher.IncludeSubdirectories = true;
            _fileWatcher.Changed += new FileSystemEventHandler(_fileWatcher_Changed);
            _fileWatcher.Created += new FileSystemEventHandler(_fileWatcher_Created);
            _fileWatcher.Deleted += new FileSystemEventHandler(_fileWatcher_Deleted);
            _fileWatcher.Renamed += new RenamedEventHandler(_fileWatcher_Renamed);
            _fileWatcher.EnableRaisingEvents = true;
        }

        void _fileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Initialise(_fileWatcher.Path);
        }

        void _fileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Initialise(_fileWatcher.Path);
        }

        void _fileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Initialise(_fileWatcher.Path);
        }

        void _fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Initialise(_fileWatcher.Path);
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

            log.DebugFormat("Loaded page content from {0}", contentFilePath);
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