import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { buttonVariants } from "@/components/ui/button";
import Link from "next/link";
import { formatMoney } from "@/lib/money";
import type { MembershipPlan } from "@/lib/types";

export function PricingCards({ plans }: { plans: MembershipPlan[] }) {
  const activePlans = plans.filter((plan) => plan.isActive).sort((a, b) => a.sortOrder - b.sortOrder);

  if (activePlans.length === 0) {
    return <p className="text-center text-sm text-muted-foreground">Membership plans are coming soon.</p>;
  }

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {activePlans.map((plan) => (
        <Card key={plan.id} className="flex flex-col">
          <CardHeader>
            <CardTitle>{plan.name}</CardTitle>
            <CardDescription>{plan.description}</CardDescription>
          </CardHeader>
          <CardContent className="flex-1">
            <p className="text-3xl font-bold">
              {formatMoney(plan.priceCents, plan.currency)}
              <span className="text-sm font-normal text-muted-foreground">
                /{plan.billingInterval === "Monthly" ? "mo" : "yr"}
              </span>
            </p>
          </CardContent>
          <CardFooter>
            <Link href="/register" className={buttonVariants({ className: "w-full" })}>
              Join now
            </Link>
          </CardFooter>
        </Card>
      ))}
    </div>
  );
}
