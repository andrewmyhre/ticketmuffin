namespace GroupGiving.Core.Actions.CancelEvent
{
    public class CancelEventResponse
    {
        private readonly bool _success;
        public bool Success { get { return _success; } }

        public CancelEventResponse()
        {
        }
        private CancelEventResponse(bool success)
        {
            _success = success;
        }
        public static CancelEventResponse Successful
        {
            get
            {
                return new CancelEventResponse(true);
            }
        }
        public static CancelEventResponse Failed
        {
            get
            {
                return new CancelEventResponse(false);
            }
        }

        
    }
}