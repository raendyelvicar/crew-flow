"use server";

import { revalidatePath } from "next/cache";
import { auth } from "@/lib/auth";
import { apiClient, ApiError } from "@/lib/api-client";

export async function bookClass(classOccurrenceId: string): Promise<{ success: boolean; message: string }> {
  const session = await auth();
  if (!session?.memberId) {
    return { success: false, message: "You need a member profile to book classes." };
  }

  try {
    const booking = await apiClient.post<{ status: string }>("/api/v1/bookings", {
      classOccurrenceId,
      memberId: session.memberId,
    });
    revalidatePath("/classes");
    revalidatePath("/bookings");
    return {
      success: true,
      message: booking.status === "Waitlisted" ? "Class is full - you've been added to the waitlist." : "You're booked!",
    };
  } catch (err) {
    if (err instanceof ApiError) {
      return { success: false, message: err.problem?.detail ?? err.message };
    }
    return { success: false, message: "Something went wrong booking this class." };
  }
}

export async function cancelBooking(bookingId: string): Promise<{ success: boolean; message: string }> {
  try {
    await apiClient.post(`/api/v1/bookings/${bookingId}/cancel`);
    revalidatePath("/classes");
    revalidatePath("/bookings");
    return { success: true, message: "Booking cancelled." };
  } catch (err) {
    if (err instanceof ApiError) {
      return { success: false, message: err.problem?.detail ?? err.message };
    }
    return { success: false, message: "Something went wrong cancelling this booking." };
  }
}
