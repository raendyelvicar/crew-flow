namespace CrewFlow.Domain.Billing;

public enum SubscriptionStatus
{
    Trialing,
    Active,
    PastDue,
    Canceled,
    Unpaid,
    Incomplete,
    IncompleteExpired
}
