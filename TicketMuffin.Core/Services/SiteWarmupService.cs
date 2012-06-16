using System;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using log4net;

namespace TicketMuffin.Core.Services
{
    public class SiteWarmupService : IWindowsService
    {
        private ILog _logger = LogManager.GetLogger(typeof (SiteWarmupService));
        private Timer _warmupTimer;
        public string RemoteUrl { get; set; }

        public SiteWarmupService()
        {
            _warmupTimer = new Timer(60000);
            RemoteUrl = "http://localhost";
        }

        public SiteWarmupService(string remoteUrl = "http://localhost:56840")
            : this()
        {
            RemoteUrl = remoteUrl;
        }

        private void MakeRequest()
        {
            new TaskFactory()
                .StartNew(() =>
                              {
                                  HttpWebRequest request = HttpWebRequest.Create(RemoteUrl) as HttpWebRequest;
                                  try
                                  {
                                      request.GetResponse();
                                  }
                                  catch (Exception ex)
                                  {
                                      _logger.Error("Site warmup failed for " + RemoteUrl, ex);
                                      if (_warmupTimer.Interval < 60000 * 60)
                                          _warmupTimer.Interval *= 2;
                                  }

                              });
        }

        public void Start()
        {
            _warmupTimer.Elapsed += (sender, args) => MakeRequest();
            _warmupTimer.Start();
            MakeRequest();
        }

        public void Stop()
        {
            _warmupTimer.Stop();
        }
    }
}