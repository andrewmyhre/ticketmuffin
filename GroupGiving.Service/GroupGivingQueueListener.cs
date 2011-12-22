using GroupGiving.Core.Services;
using log4net;

namespace GroupGiving.Service
{
    public class GroupGivingQueueListener : IAsyncListenerCommand
    {
        private ILog _log = LogManager.GetLogger(typeof (GroupGivingQueueListener));
        public void Execute<T>(T msmqMessage) where T : class
        {
            var command = msmqMessage as QueuedCommand;
            if (command == null)
            {
                _log.Warn("queue message is not of type QueuedCommand");
                return;
            }

            _log.DebugFormat("processing queue command {0}", command.Action);
        }

        public void SendToDeadLetterQueue<T>(T msmqMessage, string exception) where T : class
        {
            
        }
    }
}