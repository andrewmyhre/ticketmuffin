using System;
using System.Net;
using System.Timers;
using log4net;

namespace GroupGiving.Core.Services
{
    public class SiteWarmupService : IWindowsService
    {
        private ILog _logger = LogManager.GetLogger(typeof (SiteWarmupService));
        private Timer _warmupTimer;
        public string RemoteUrl { get; set; }

        public SiteWarmupService(string remoteUrl="localhost")
        {
            _warmupTimer = new Timer(60000);
            RemoteUrl = remoteUrl;
            _warmupTimer.Elapsed += (sender, args) =>
                                        {
                                            HttpWebRequest request = HttpWebRequest.Create(RemoteUrl) as HttpWebRequest;
                                            try
                                            {
                                                request.GetResponse();
                                            } catch (Exception ex)
                                            {
                                                _logger.Error("Site warmup failed", ex);
                                            }
                                        };
        }

        public void Start()
        {
            _warmupTimer.Start();
        }

        public void Stop()
        {
            _warmupTimer.Stop();
        }
    }
}