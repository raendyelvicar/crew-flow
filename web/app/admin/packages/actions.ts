"use server";

import { revalidatePath } from "next/cache";
import { apiClient, ApiError } from "@/lib/api-client";
import type { BillingInterval } from "@/lib/types";

export type PlanFormInput = {
  name: string;
  description: string;
  billingInterval: BillingInterval;
  priceAmount: string;
  isActive: boolean;
};

export async function savePlan(id: string | null, form: PlanFormInput) {
  const payload = {
    name: form.name,
    description: form.description || undefined,
    billingInterval: form.billingInterval,
    priceAmount: Number(form.priceAmount),
    currency: "idr",
    isActive: form.isActive,
    sortOrder: 0,
  };

  try {
    if (id) await apiClient.put(`/api/v1/membership-plans/${id}`, payload);
    else await apiClient.post("/api/v1/membership-plans", payload);
    revalidatePath("/admin/packages");
    return { success: true, message: "Membership plan saved." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}

export type CreditPackFormInput = {
  name: string;
  description: string;
  creditCount: string;
  priceAmount: string;
  expiryDays: string;
  isActive: boolean;
};

export async function saveCreditPack(id: string | null, form: CreditPackFormInput) {
  const payload = {
    name: form.name,
    description: form.description || undefined,
    creditCount: Number(form.creditCount),
    priceAmount: Number(form.priceAmount),
    currency: "idr",
    expiryDays: form.expiryDays ? Number(form.expiryDays) : undefined,
    isActive: form.isActive,
  };

  try {
    if (id) await apiClient.put(`/api/v1/credit-packs/${id}`, payload);
    else await apiClient.post("/api/v1/credit-packs", payload);
    revalidatePath("/admin/packages");
    return { success: true, message: "Credit pack saved." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
