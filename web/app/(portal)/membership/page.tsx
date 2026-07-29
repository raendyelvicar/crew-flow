import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { SubscribeButton, BuyCreditsButton, CancelSubscriptionButton } from "@/components/membership-actions";
import { apiClient, ApiError, publicGet } from "@/lib/api-client";
import type { CreditPack, CreditPackPurchase, MembershipPlan, Subscription } from "@/lib/types";

async function safeGet<T>(promise: Promise<T>, fallback: T): Promise<T> {
  try {
    return await promise;
  } catch (err) {
    if (err instanceof ApiError && (err.status === 404 || err.status === 409)) return fallback;
    throw err;
  }
}

function formatPrice(cents: number, currency: string) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: currency.toUpperCase() }).format(cents / 100);
}

export default async function MembershipPage() {
  const [subscription, plans, creditPacks, purchases] = await Promise.all([
    safeGet<Subscription | null>(apiClient.get("/api/v1/subscriptions/me"), null),
    publicGet<MembershipPlan[]>("/api/v1/membership-plans?activeOnly=true").then((r) => r ?? []),
    publicGet<CreditPack[]>("/api/v1/credit-packs?activeOnly=true").then((r) => r ?? []),
    safeGet<CreditPackPurchase[]>(apiClient.get("/api/v1/credit-packs/purchases/me"), []),
  ]);

  const activeCredits = purchases.filter((p) => p.status === "Active").reduce((sum, p) => sum + p.creditsRemaining, 0);

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Membership</h1>
        <p className="text-sm text-muted-foreground">Manage your subscription and drop-in credits.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Current status</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          {subscription ? (
            <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <p className="font-medium">{subscription.planName}</p>
                <p className="text-sm text-muted-foreground">
                  {subscription.currentPeriodEndUtc &&
                    `Renews ${new Date(subscription.currentPeriodEndUtc).toLocaleDateString()}`}
                </p>
              </div>
              <div className="flex items-center gap-3">
                <Badge>{subscription.status}</Badge>
                {!subscription.cancelAtPeriodEnd && <CancelSubscriptionButton subscriptionId={subscription.id} />}
              </div>
            </div>
          ) : (
            <p className="text-sm text-muted-foreground">No active subscription - choose a plan below.</p>
          )}
          <p className="text-sm text-muted-foreground">{activeCredits} drop-in credit(s) remaining</p>
        </CardContent>
      </Card>

      <div>
        <h2 className="mb-3 text-lg font-semibold">Membership plans</h2>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {plans.map((plan) => (
            <Card key={plan.id}>
              <CardHeader>
                <CardTitle>{plan.name}</CardTitle>
                <CardDescription>{plan.description}</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <p className="text-2xl font-bold">
                  {formatPrice(plan.priceCents, plan.currency)}
                  <span className="text-sm font-normal text-muted-foreground">
                    /{plan.billingInterval === "Monthly" ? "mo" : "yr"}
                  </span>
                </p>
                <SubscribeButton membershipPlanId={plan.id} />
              </CardContent>
            </Card>
          ))}
        </div>
      </div>

      <div>
        <h2 className="mb-3 text-lg font-semibold">Drop-in credit packs</h2>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {creditPacks.map((pack) => (
            <Card key={pack.id}>
              <CardHeader>
                <CardTitle>{pack.name}</CardTitle>
                <CardDescription>{pack.creditCount} classes{pack.expiryDays ? ` - expires in ${pack.expiryDays} days` : ""}</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <p className="text-2xl font-bold">{formatPrice(pack.priceCents, pack.currency)}</p>
                <BuyCreditsButton creditPackId={pack.id} />
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </div>
  );
}
