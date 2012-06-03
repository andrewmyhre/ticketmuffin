using System;
using System.Messaging;
using System.Threading;
using log4net;

namespace GroupGiving.Service
{
    public class MsmqAsyncListener<T> : IMsmqListener, IDisposable where T : class
    {
        private ILog _log = LogManager.GetLogger(typeof (MsmqAsyncListener<T>));
        private MessageQueue _queue;
        private readonly IAsyncListenerCommand _command;
        private readonly bool _useCountProcessedMessages;
        private readonly bool _useThreadPool;
        private readonly bool _useExceptionDeadLetterQueues;

        public MsmqAsyncListener(IAsyncListenerCommand command)
        {
            _command = command;
            _useCountProcessedMessages = true;
            _useThreadPool = false;
            _useExceptionDeadLetterQueues = true;

            if (_useCountProcessedMessages)
            {
                MsmqAsyncListenerHelper.SetupMessageProcessedCountList();
            }
        }

        public MsmqAsyncListener(IAsyncListenerCommand command, bool useCountProcessedMessages, bool useThreadPool, bool useExceptionDeadLetterQueues)
        {
            _command = command;

            _useCountProcessedMessages = useCountProcessedMessages;
            _useThreadPool = useThreadPool;
            _useExceptionDeadLetterQueues = useExceptionDeadLetterQueues;

            if (_useCountProcessedMessages)
            {
                MsmqAsyncListenerHelper.SetupMessageProcessedCountList();
            }
        }

        public void SetListener(string queuePath)
        {
            _queue = new MessageQueue(queuePath);
            _queue.ReceiveCompleted += Queue_ReceiveCompleted;
            _queue.Formatter = new XmlMessageFormatter(new[] { typeof(T) });

            _queue.BeginReceive();
        }

        public void Queue_ReceiveCompleted(object source, EventArgs asyncResult)
        {
            var result = asyncResult as ReceiveCompletedEventArgs;
            var item = default(T);

            MessageQueue queue;
            try
            {
                queue = (MessageQueue)source;
                var message = queue.EndReceive(result.AsyncResult);

                // Read the item from the message body
                item = (T)message.Body;

                if (_useThreadPool)
                {
                    // Increases the throughput of messages
                    // however, needs to be throttled by having less listeners in the 
                    // config or Quantiv will die. 
                    // ie: reduce EmailServiceListenerCount="..." and PaymentServiceListenerCount="..."
                    // sections in App.config  
                    ThreadPool.QueueUserWorkItem(ExecuteCommand, item);
                }
                else
                {
                    _command.Execute(item);
                }
            }
            catch (Exception ex)
            {
                _log.Warn(ex);

                if (_useExceptionDeadLetterQueues)
                {
                    _command.SendToDeadLetterQueue(item, ex.ToString());
                }

            }
            finally
            {
                if (_useCountProcessedMessages)
                {
                    MsmqAsyncListenerHelper.AddOneToProcessedMessageCount();
                }

                //Call this to continue processing
                _queue.BeginReceive();
            }
        }

        public void ExecuteCommand(Object stateInfo)
        {
            T item = default(T);

            try
            {
                item = (T)stateInfo;
                _command.Execute(item);
            }
            catch (Exception ex)
            {
                _log.Warn(ex);

                if (_useExceptionDeadLetterQueues)
                {
                    _command.SendToDeadLetterQueue(item, ex.ToString());
                }
            }
        }

        public void Dispose()
        {
            _queue.ReceiveCompleted -= Queue_ReceiveCompleted;
            _queue.Close();
        }

    }
}