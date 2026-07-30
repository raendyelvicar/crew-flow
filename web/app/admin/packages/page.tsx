import { Badge } from "@/components/ui/badge";
import { CreditPackDialog } from "@/components/credit-pack-dialog";
import { PlanDialog } from "@/components/plan-dialog";
import { apiClient } from "@/lib/api-client";
import { formatMoney } from "@/lib/money";
import type { CreditPack, MembershipPlan } from "@/lib/types";

export default async function PackagesPage() {
  const [plans, packs] = await Promise.all([
    apiClient.get<MembershipPlan[]>("/api/v1/membership-plans?activeOnly=false"),
    apiClient.get<CreditPack[]>("/api/v1/credit-packs?activeOnly=false"),
  ]);

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Packages</h1>
        <p className="text-sm text-muted-foreground">Membership plans and drop-in credit packs.</p>
      </div>

      <div>
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-lg font-semibold">Membership plans</h2>
          <PlanDialog />
        </div>
        <div className="space-y-2">
          {plans.map((plan) => (
            <div key={plan.id} className="flex items-center justify-between rounded-lg border p-4">
              <div>
                <div className="flex items-center gap-2">
                  <p className="font-medium">{plan.name}</p>
                  <Badge variant={plan.isActive ? "default" : "secondary"}>{plan.isActive ? "Active" : "Inactive"}</Badge>
                </div>
                <p className="text-sm text-muted-foreground">
                  {formatMoney(plan.priceCents, plan.currency)} / {plan.billingInterval === "Monthly" ? "mo" : "yr"}
                </p>
              </div>
              <PlanDialog plan={plan} />
            </div>
          ))}
        </div>
      </div>

      <div>
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-lg font-semibold">Credit packs</h2>
          <CreditPackDialog />
        </div>
        <div className="space-y-2">
          {packs.map((pack) => (
            <div key={pack.id} className="flex items-center justify-between rounded-lg border p-4">
              <div>
                <div className="flex items-center gap-2">
                  <p className="font-medium">{pack.name}</p>
                  <Badge variant={pack.isActive ? "default" : "secondary"}>{pack.isActive ? "Active" : "Inactive"}</Badge>
                </div>
                <p className="text-sm text-muted-foreground">
                  {formatMoney(pack.priceCents, pack.currency)} - {pack.creditCount} credits
                  {pack.expiryDays ? ` - expires in ${pack.expiryDays} days` : ""}
                </p>
              </div>
              <CreditPackDialog pack={pack} />
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
