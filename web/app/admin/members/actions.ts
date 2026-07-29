"use server";

import { revalidatePath } from "next/cache";
import { apiClient, ApiError } from "@/lib/api-client";

export type CreateMemberInput = {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
};

export async function createMember(payload: CreateMemberInput) {
  try {
    await apiClient.post("/api/v1/members", payload);
    revalidatePath("/admin/members");
    return { success: true, message: "Member created." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
