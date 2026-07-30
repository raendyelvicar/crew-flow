// `priceAmount` is always the literal amount in the currency's major unit (e.g. 400000
// means Rp400,000) - the app never deals in "cents". Stripe's smallest-unit convention only
// applies at the Stripe API boundary (see StripeService.ToStripeUnitAmount on the backend).
export function formatMoney(priceAmount: number, currency: string): string {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: currency.toUpperCase() }).format(priceAmount);
}
