using CrewFlow.Domain.Members;

namespace CrewFlow.Domain.Billing;

public class Subscription
{
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }
    public Member? Member { get; set; }

    public Guid MembershipPlanId { get; set; }
    public MembershipPlan? MembershipPlan { get; set; }

    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Incomplete;
    public DateTime? CurrentPeriodStartUtc { get; set; }
    public DateTime? CurrentPeriodEndUtc { get; set; }

    // Reset to MembershipPlan.CreditsPerPeriod whenever the current billing period rolls over
    // (see StripeWebhookProcessingService.HandleSubscriptionUpsertAsync) - unused credits are
    // forfeited at renewal, they do not carry over.
    public int CreditsRemainingThisPeriod { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CanceledAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public static readonly SubscriptionStatus[] NonTerminalStatuses =
    [
        SubscriptionStatus.Trialing,
        SubscriptionStatus.Active,
        SubscriptionStatus.PastDue
    ];

    public bool IsActiveForBooking => Status is SubscriptionStatus.Active or SubscriptionStatus.Trialing;
}
