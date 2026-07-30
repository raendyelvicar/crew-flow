import { PricingCards } from "@/components/pricing-cards";
import { CreditPackCards } from "@/components/credit-pack-cards";
import { publicGet } from "@/lib/api-client";
import type { CreditPack, MembershipPlan } from "@/lib/types";

export const dynamic = "force-dynamic";

export default async function PricingPage() {
  const [plans, packs] = await Promise.all([
    publicGet<MembershipPlan[]>("/api/v1/membership-plans?activeOnly=true").then((r) => r ?? []),
    publicGet<CreditPack[]>("/api/v1/credit-packs?activeOnly=true").then((r) => r ?? []),
  ]);

  return (
    <section className="px-4 py-12 sm:py-16">
      <div className="mx-auto max-w-5xl space-y-12">
        <div>
          <div className="mb-8 text-center">
            <h1 className="text-2xl font-bold tracking-tight sm:text-4xl">Membership</h1>
            <p className="mt-2 text-sm text-muted-foreground sm:text-base">
              Choose a monthly plan with a set number of classes, or a drop-in credit pack below.
            </p>
          </div>
          <PricingCards plans={plans} />
        </div>

        {packs.length > 0 && (
          <div>
            <div className="mb-8 text-center">
              <h2 className="text-xl font-bold tracking-tight sm:text-2xl">Drop-in credit packs</h2>
              <p className="mt-2 text-sm text-muted-foreground sm:text-base">
                Not ready to commit? Buy a pack of classes and use them whenever you like.
              </p>
            </div>
            <CreditPackCards packs={packs} />
          </div>
        )}
      </div>
    </section>
  );
}
