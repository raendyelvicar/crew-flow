import { PricingCards } from "@/components/pricing-cards";
import { publicGet } from "@/lib/api-client";
import type { MembershipPlan } from "@/lib/types";

export const dynamic = "force-dynamic";

export default async function PricingPage() {
  const plans = (await publicGet<MembershipPlan[]>("/api/v1/membership-plans?activeOnly=true")) ?? [];

  return (
    <section className="px-4 py-12 sm:py-16">
      <div className="mx-auto max-w-5xl">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-bold tracking-tight sm:text-4xl">Membership</h1>
          <p className="mt-2 text-sm text-muted-foreground sm:text-base">
            Choose a monthly or annual plan, or grab a drop-in credit pack from your member dashboard.
          </p>
        </div>
        <PricingCards plans={plans} />
      </div>
    </section>
  );
}
