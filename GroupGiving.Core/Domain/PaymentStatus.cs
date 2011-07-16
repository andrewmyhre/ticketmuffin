namespace GroupGiving.Core.Domain
{
    public enum PaymentStatus
    {
        Unpaid,
        PaidPendingReconciliation,
        Reconciled,
        Refunded
    }
}