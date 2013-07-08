using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Raven.Client;
using TicketMuffin.Core.Conventions;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;
using Raven.Client.Linq;
using log4net;

namespace TicketMuffin.Core.Test.Integration
{
    [TestFixture]
    public class ContentTests
    {
        [SetUp]
        public void Setup()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        [Test]
        public void WhenRequestingANonExistingPage_PageIsCreated()
        {
            using (var store = RavenStore.CreateDocumentStore())
            using (var session = store.OpenSession())
            {
                IContentProvider cp = new RavenDbContentProvider(session);
                string pageAddress = Guid.NewGuid().ToString();
                PageContent content = null;
                string label = "";
                cp.GetContent(pageAddress, "main text", "default content", "en-GB");
            }
        }
        [Test]
        public void WhenRequestingANonExistingPageWithSeveralContents_ContentIsCreated()
        {
            using (var store = RavenStore.CreateDocumentStore())
            using (var session = store.OpenSession())
            {
                IContentProvider cp = new RavenDbContentProvider(session);
                string pageAddress = Guid.NewGuid().ToString();
                PageContent content = null;
                string label = "";
                cp.GetContent(pageAddress, "text1", "default content", "en-GB");
                cp.GetContent(pageAddress, "text2", "default content", "en-GB");
                cp.GetContent(pageAddress, "text3", "default content", "en-GB");
                cp.GetContent(pageAddress, "text4", "default content", "en-GB");
                cp.GetContent(pageAddress, "text5", "default content", "en-GB");
                cp.GetContent(pageAddress, "text6", "default content", "en-GB");
                session.SaveChanges();

                var contentItems = session.Query<LocalisedContent>().Customize(x=>x.WaitForNonStaleResults(TimeSpan.FromSeconds(5)));
                string[] pages = contentItems.Select(c => c.Address).Distinct().ToArray();
                Assert.That(pages.Length, Is.EqualTo(1));
                Assert.That(contentItems.Count(), Is.EqualTo(6));
            }
        }

        [Test]
        public void WhenRequestingAContentOnTwoThreads_ONlyOneContentIsCreated()
        {
            var logger = LogManager.GetLogger(typeof (ContentTests));
            using (var store = RavenStore.CreateDocumentStore())
            {
                string pageAddress = "/test/page/url";
                var task1 = new Task(() =>
                    {
                        logger.Info("task 1 start");
                        using (var session = store.OpenSession())
                        using (var contentProvider = new RavenDbContentProvider(session))
                        {
                            logger.Info("get content " + pageAddress);
                            contentProvider.GetContent(pageAddress, "main text", "default content", "en-GB");
                            contentProvider.GetContent(pageAddress, "text2", "default content", "en-GB");
                            contentProvider.GetContent(pageAddress, "text3", "default content", "en-GB");
                            logger.Info("got content " + pageAddress);
                        }
                    });

                var task2 = new Task(() =>
                    {
                        logger.Info("task 2 start");
                        using (var session = store.OpenSession())
                        using (var contentProvider = new RavenDbContentProvider(session))
                        {
                            logger.Info("get content " + pageAddress);
                            contentProvider.GetContent(pageAddress, "main text", "default content", "en-GB");
                            contentProvider.GetContent(pageAddress, "text2", "default content", "en-GB");
                            contentProvider.GetContent(pageAddress, "text3", "default content", "en-GB");
                            logger.Info("got content " + pageAddress);
                        }
                    });

                task1.Start(TaskScheduler.Default);
                task2.Start(TaskScheduler.Default);
                Task.WaitAll(task1, task2);

                using (var session = store.OpenSession())
                {
                    RavenQueryStatistics stats;
                    var content = session
                        .Query<LocalisedContent>()
                        .Statistics(out stats)
                        .Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(5)))
                        .ToArray();

                    Assert.That(stats.TotalResults, Is.EqualTo(3));
                    foreach (var c in content)
                    {
                        logger.InfoFormat("content: {0}", c.Id);
                        logger.InfoFormat("culture: {0}", c.Culture);
                        logger.InfoFormat("address: {0}", c.Address);
                        logger.InfoFormat("label: {0}", c.Label);
                    }
                }
            }



        }
    }
}
