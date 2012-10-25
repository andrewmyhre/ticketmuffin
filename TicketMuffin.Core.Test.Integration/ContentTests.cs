using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TicketMuffin.Core.Conventions;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;
using Raven.Client.Linq;

namespace TicketMuffin.Core.Test.Integration
{
    [TestFixture]
    public class ContentTests
    {
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
                cp.GetContent(pageAddress, "main text", "default content", "en-GB", out content, out label);
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
                cp.GetContent(pageAddress, "text1", "default content", "en-GB", out content, out label);
                cp.GetContent(pageAddress, "text2", "default content", "en-GB", out content, out label);
                cp.GetContent(pageAddress, "text3", "default content", "en-GB", out content, out label);
                cp.GetContent(pageAddress, "text4", "default content", "en-GB", out content, out label);
                cp.GetContent(pageAddress, "text5", "default content", "en-GB", out content, out label);
                cp.GetContent(pageAddress, "text6", "default content", "en-GB", out content, out label);
                session.SaveChanges();

                var cs = session.Query<PageContent>().Where(pc => pc.Address == pageAddress).Customize(x=>x.WaitForNonStaleResults(TimeSpan.FromSeconds(5)));
                Assert.That(cs.Count(), Is.EqualTo(1));

                foreach(var c in cs)
                {
                    session.Delete(c);
                }
                session.SaveChanges();
            }
        }

        [Test]
        public void WhenRequestingAContentOnTwoThreads_ONlyOneContentIsCreated()
        {
            using (var store = RavenStore.CreateDocumentStore())
            {

                string pageAddress = Guid.NewGuid().ToString();
                var task1 = new Task(() =>
                    {
                        System.Diagnostics.Debug.WriteLine("task 1 start");
                        using (var session = store.OpenSession())
                        {
                            IContentProvider cp = new RavenDbContentProvider(session);
                            PageContent content = null;
                            string label = "";
                            cp.GetContent(pageAddress, "main text", "default content", "en-GB", out content, out label);
                            cp.GetContent(pageAddress, "text2", "default content", "en-GB", out content, out label);
                            cp.GetContent(pageAddress, "text3", "default content", "en-GB", out content, out label);
                            session.SaveChanges();
                            System.Diagnostics.Debug.WriteLine("got content " + pageAddress);
                        }
                    });

                var task2 = new Task(() =>
                    {
                        System.Diagnostics.Debug.WriteLine("task 2 start");
                        using (var session = store.OpenSession())
                        {
                            IContentProvider cp = new RavenDbContentProvider(session);
                            PageContent content = null;
                            string label = "";
                            cp.GetContent(pageAddress, "main text", "default content", "en-GB", out content, out label);
                            cp.GetContent(pageAddress, "text2", "default content", "en-GB", out content, out label);
                            cp.GetContent(pageAddress, "text3", "default content", "en-GB", out content, out label);
                            session.SaveChanges();
                            System.Diagnostics.Debug.WriteLine("got content " + pageAddress);
                        }
                    });

                task1.Start(TaskScheduler.Default);
                task2.Start(TaskScheduler.Default);
                Task.WaitAll(task1, task2);

                using (var session = store.OpenSession())
                {
                    IContentProvider cp = new RavenDbContentProvider(session);

                    RavenQueryStatistics stats;
                    var content = session
                        .Query<PageContent>()
                        .Statistics(out stats)
                        .Where(c => c.Address == pageAddress)
                        .Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(5)));

                    Assert.That(content.Count(), Is.EqualTo(1));
                    System.Diagnostics.Debug.WriteLine("count = " + content.Count());

                    foreach (var c in content)
                    {
                        session.Delete(c);
                    }
                    session.SaveChanges();
                }
            }



        }
    }
}
