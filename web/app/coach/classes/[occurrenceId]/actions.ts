"use server";

import { revalidatePath } from "next/cache";
import { apiClient, ApiError } from "@/lib/api-client";

// Mirrors the admin roster's markAttendance action - kept separate (rather than imported
// cross-route) so each route owns the path it revalidates.
export async function markAttendance(occurrenceId: string, status: "Attended" | "NoShow", bookingId: string) {
  try {
    await apiClient.post(`/api/v1/bookings/${bookingId}/attendance`, { status });
    revalidatePath(`/coach/classes/${occurrenceId}`);
    return { success: true, message: `Marked ${status}.` };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
