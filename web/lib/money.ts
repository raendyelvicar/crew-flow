// Currencies where the smallest unit IS the display unit (no /100 division) -
// matters now that real IDR pricing is seeded, not just USD test data.
const ZERO_DECIMAL_CURRENCIES = new Set([
  "idr", "jpy", "krw", "vnd", "clp", "isk", "huf", "twd", "ugx",
  "bif", "djf", "gnf", "kmf", "xaf", "xof", "xpf", "pyg", "rwf", "vuv",
]);

// `amountInSmallestUnit` matches Stripe's unit-amount convention (e.g. MembershipPlan.priceCents).
export function formatMoney(amountInSmallestUnit: number, currency: string): string {
  const isZeroDecimal = ZERO_DECIMAL_CURRENCIES.has(currency.toLowerCase());
  const value = isZeroDecimal ? amountInSmallestUnit : amountInSmallestUnit / 100;
  return new Intl.NumberFormat("en-US", { style: "currency", currency: currency.toUpperCase() }).format(value);
}
