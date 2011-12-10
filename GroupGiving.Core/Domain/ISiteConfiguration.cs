namespace GroupGiving.Core.Domain
{
    public interface ISiteConfiguration
    {
        string Id { get; set; }
        string EventImagePathFormat { get; set; }
        string LoginUrl { get; set; }
        DatabaseConfiguration DatabaseConfiguration { get; set; }
        JustGivingApiConfiguration JustGivingApiConfiguration { get; set; }
        PayFlowProConfiguration PayFlowProConfiguration { get; set; }
        AdaptiveAccountsConfiguration AdaptiveAccountsConfiguration { get; set; }
    }
}