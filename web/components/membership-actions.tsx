"use client";

import { useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { buyCreditPack, cancelSubscription, subscribeToPlan } from "@/app/(portal)/membership/actions";

export function SubscribeButton({ membershipPlanId }: { membershipPlanId: string }) {
  const [isPending, startTransition] = useTransition();
  return (
    <Button
      className="w-full"
      disabled={isPending}
      onClick={() =>
        startTransition(async () => {
          const result = await subscribeToPlan(membershipPlanId);
          if (result && !result.success) toast.error(result.message);
        })
      }
    >
      {isPending ? "Redirecting to checkout..." : "Subscribe"}
    </Button>
  );
}

export function BuyCreditsButton({ creditPackId }: { creditPackId: string }) {
  const [isPending, startTransition] = useTransition();
  return (
    <Button
      className="w-full"
      variant="outline"
      disabled={isPending}
      onClick={() =>
        startTransition(async () => {
          const result = await buyCreditPack(creditPackId);
          if (result && !result.success) toast.error(result.message);
        })
      }
    >
      {isPending ? "Redirecting to checkout..." : "Buy pack"}
    </Button>
  );
}

export function CancelSubscriptionButton({ subscriptionId }: { subscriptionId: string }) {
  const [isPending, startTransition] = useTransition();
  return (
    <Button
      size="sm"
      variant="outline"
      disabled={isPending}
      onClick={() =>
        startTransition(async () => {
          const result = await cancelSubscription(subscriptionId);
          if (result.success) toast.success(result.message);
          else toast.error(result.message);
        })
      }
    >
      {isPending ? "Cancelling..." : "Cancel at period end"}
    </Button>
  );
}
