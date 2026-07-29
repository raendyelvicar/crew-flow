"use server";

import { revalidatePath } from "next/cache";
import { apiClient, ApiError } from "@/lib/api-client";

export async function markAttendance(bookingId: string, occurrenceId: string, status: "Attended" | "NoShow") {
  try {
    await apiClient.post(`/api/v1/bookings/${bookingId}/attendance`, { status });
    revalidatePath(`/admin/schedule/occurrences/${occurrenceId}`);
    return { success: true, message: `Marked ${status}.` };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
