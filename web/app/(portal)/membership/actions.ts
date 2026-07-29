"use server";

import { redirect } from "next/navigation";
import { auth } from "@/lib/auth";
import { apiClient, ApiError } from "@/lib/api-client";

const APP_URL = process.env.NEXTAUTH_URL ?? "http://localhost:3000";

export async function subscribeToPlan(membershipPlanId: string) {
  const session = await auth();
  if (!session?.memberId) return { success: false, message: "You need a member profile to subscribe." };

  try {
    const { checkoutUrl } = await apiClient.post<{ checkoutUrl: string }>("/api/v1/subscriptions/checkout", {
      memberId: session.memberId,
      membershipPlanId,
      successUrl: `${APP_URL}/membership?checkout=success`,
      cancelUrl: `${APP_URL}/membership?checkout=cancelled`,
    });
    redirect(checkoutUrl);
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    throw err;
  }
}

export async function buyCreditPack(creditPackId: string) {
  const session = await auth();
  if (!session?.memberId) return { success: false, message: "You need a member profile to buy credits." };

  try {
    const { checkoutUrl } = await apiClient.post<{ checkoutUrl: string }>("/api/v1/credit-packs/checkout", {
      memberId: session.memberId,
      creditPackId,
      successUrl: `${APP_URL}/membership?checkout=success`,
      cancelUrl: `${APP_URL}/membership?checkout=cancelled`,
    });
    redirect(checkoutUrl);
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    throw err;
  }
}

export async function cancelSubscription(subscriptionId: string) {
  try {
    await apiClient.post(`/api/v1/subscriptions/${subscriptionId}/cancel?atPeriodEnd=true`);
    return { success: true, message: "Your subscription will end at the current period." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
