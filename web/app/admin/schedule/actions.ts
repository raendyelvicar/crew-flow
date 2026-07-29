"use server";

import { revalidatePath } from "next/cache";
import { apiClient, ApiError } from "@/lib/api-client";

export type CreateActivityInput = {
  name: string;
  description?: string;
  category: string;
  defaultCapacity: number;
  defaultDurationMinutes: number;
};

export async function createActivity(input: CreateActivityInput) {
  try {
    await apiClient.post("/api/v1/activities", { ...input, isActive: true });
    revalidatePath("/admin/schedule");
    return { success: true, message: "Activity created." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}

export type CreateClassScheduleInput = {
  activityId: string;
  instructorUserId: string;
  dayOfWeek: number;
  startTimeLocal: string;
  durationMinutes: number;
  capacity: number;
  timezone: string;
};

export async function createClassSchedule(input: CreateClassScheduleInput) {
  try {
    await apiClient.post("/api/v1/class-schedules", {
      ...input,
      effectiveFromDate: new Date().toISOString().slice(0, 10),
    });
    revalidatePath("/admin/schedule");
    return { success: true, message: "Class schedule created - occurrences generated for the next 8 weeks." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
