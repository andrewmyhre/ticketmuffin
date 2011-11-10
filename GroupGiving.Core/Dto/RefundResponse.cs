namespace GroupGiving.Core.Dto
{
    public class RefundResponse : ResponseBase
    {
        public bool Successful { get; set; }

        public object RawResponse { get; set; }

        public DialogueHistoryEntry DialogueEntry { get; set; }

        public string Message { get; set; }
    }
}