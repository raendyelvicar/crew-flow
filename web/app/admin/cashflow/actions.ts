"use server";

import { revalidatePath } from "next/cache";
import { apiClient, ApiError } from "@/lib/api-client";

export type CreateCashflowEntryInput = {
  amount: number;
  source: "ManualCash" | "ManualCard" | "Other";
  category: "Membership" | "CreditPack" | "DropIn" | "Merchandise" | "Other";
  description?: string;
};

export async function createCashflowEntry(input: CreateCashflowEntryInput) {
  const payload = { ...input, currency: "usd", occurredAtUtc: new Date().toISOString() };

  try {
    await apiClient.post("/api/v1/cashflow", payload);
    revalidatePath("/admin/cashflow");
    return { success: true, message: "Entry recorded." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
