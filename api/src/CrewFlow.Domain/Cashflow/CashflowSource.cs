namespace CrewFlow.Domain.Cashflow;

public enum CashflowSource
{
    StripeCharge,
    StripeInvoice,
    ManualCash,
    ManualCard,
    Other
}
