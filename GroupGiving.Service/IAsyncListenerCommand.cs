namespace GroupGiving.Service
{
    public interface IAsyncListenerCommand
    {
        void Execute<T>(T msmqMessage) where T : class;
        void SendToDeadLetterQueue<T>(T msmqMessage, string exception) where T : class;
    }
}