using System;
using System.Net;
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

        public void Start()
        {
            _warmupTimer.Elapsed += (sender, args) =>
            {
                HttpWebRequest request = HttpWebRequest.Create(RemoteUrl) as HttpWebRequest;
                try
                {
                    request.GetResponse();
                }
                catch (Exception ex)
                {
                    _logger.Error("Site warmup failed", ex);
                }
            };
            _warmupTimer.Start();
        }

        public void Stop()
        {
            _warmupTimer.Stop();
        }
    }
}