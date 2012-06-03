using System;

namespace GroupGiving.Service
{
    public interface IMsmqListener
    {
        void SetListener(string queuePath);
        void Queue_ReceiveCompleted(object source, EventArgs asyncResult);
        void ExecuteCommand(Object stateInfo);
    }
}