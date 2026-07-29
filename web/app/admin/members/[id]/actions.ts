"use server";

import { revalidatePath } from "next/cache";
import { apiClient, ApiError } from "@/lib/api-client";
import type { MemberStatus } from "@/lib/types";

export async function updateMemberStatus(memberId: string, status: MemberStatus) {
  try {
    await apiClient.patch(`/api/v1/members/${memberId}/status`, { status });
    revalidatePath(`/admin/members/${memberId}`);
    revalidatePath("/admin/members");
    return { success: true, message: "Status updated." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
