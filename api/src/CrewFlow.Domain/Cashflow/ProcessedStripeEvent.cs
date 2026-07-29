namespace CrewFlow.Domain.Cashflow;

// Primary idempotency guard for the Stripe webhook: every event id is recorded
// here before any side effects run, so a retried delivery short-circuits as a no-op.
public class ProcessedStripeEvent
{
    public Guid Id { get; set; }
    public string StripeEventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}
