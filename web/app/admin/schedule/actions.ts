"use server";

import { revalidatePath } from "next/cache";
import { apiClient, ApiError } from "@/lib/api-client";

export type ActivityFormInput = {
  name: string;
  description: string;
  classGenreId: string;
  classTypeId: string;
  defaultCapacity: string;
  defaultDurationMinutes: string;
  isActive: boolean;
};

export async function saveActivity(id: string | null, form: ActivityFormInput) {
  const payload = {
    name: form.name,
    description: form.description || undefined,
    classGenreId: form.classGenreId,
    classTypeId: form.classTypeId,
    defaultCapacity: Number(form.defaultCapacity),
    defaultDurationMinutes: Number(form.defaultDurationMinutes),
    isActive: form.isActive,
  };

  try {
    if (id) await apiClient.put(`/api/v1/activities/${id}`, payload);
    else await apiClient.post("/api/v1/activities", payload);
    revalidatePath("/admin/schedule");
    return { success: true, message: "Activity saved." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}

export type ClassScheduleFormInput = {
  activityId: string;
  instructorUserId: string;
  dayOfWeek: string;
  startTimeLocal: string;
  durationMinutes: string;
  capacity: string;
  timezone: string;
  isActive: boolean;
};

export async function createClassSchedule(form: ClassScheduleFormInput) {
  try {
    await apiClient.post("/api/v1/class-schedules", {
      activityId: form.activityId,
      instructorUserId: form.instructorUserId,
      dayOfWeek: Number(form.dayOfWeek),
      startTimeLocal: form.startTimeLocal,
      durationMinutes: Number(form.durationMinutes),
      capacity: Number(form.capacity),
      timezone: form.timezone,
      effectiveFromDate: new Date().toISOString().slice(0, 10),
    });
    revalidatePath("/admin/schedule");
    return { success: true, message: "Class schedule created - occurrences generated for the next 8 weeks." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}

export async function updateClassSchedule(id: string, form: ClassScheduleFormInput, effectiveFromDate: string) {
  try {
    await apiClient.put(`/api/v1/class-schedules/${id}`, {
      instructorUserId: form.instructorUserId,
      dayOfWeek: Number(form.dayOfWeek),
      startTimeLocal: form.startTimeLocal,
      durationMinutes: Number(form.durationMinutes),
      capacity: Number(form.capacity),
      timezone: form.timezone,
      effectiveFromDate,
      isActive: form.isActive,
    });
    revalidatePath("/admin/schedule");
    return { success: true, message: "Class schedule updated." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
