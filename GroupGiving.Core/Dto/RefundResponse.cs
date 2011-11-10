namespace GroupGiving.Core.Dto
{
    public class RefundResponse
    {
        public bool Successful { get; set; }

        public object RawResponse { get; set; }

        public DialogueHistoryEntry DialogueEntry { get; set; }
    }
}