"use server";

import { revalidatePath } from "next/cache";
import { apiClient, ApiError } from "@/lib/api-client";

export type CoachFormInput = {
  bio: string;
  avatarUrl: string;
  yearsExperience: string;
  instagramHandle: string;
  websiteUrl: string;
  danceStyleIds: string[];
};

export async function createCoach(firstName: string, lastName: string, email: string, form: CoachFormInput) {
  try {
    await apiClient.post("/api/v1/instructors", {
      firstName,
      lastName,
      email,
      bio: form.bio || undefined,
      avatarUrl: form.avatarUrl || undefined,
      yearsExperience: form.yearsExperience ? Number(form.yearsExperience) : undefined,
      instagramHandle: form.instagramHandle || undefined,
      websiteUrl: form.websiteUrl || undefined,
      danceStyleIds: form.danceStyleIds,
    });
    revalidatePath("/admin/coaches");
    return { success: true, message: "Coach created. Default password: ChangeMe123!" };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}

export async function updateCoach(userId: string, isActive: boolean, form: CoachFormInput) {
  try {
    await apiClient.put("/api/v1/instructors", {
      userId,
      bio: form.bio || undefined,
      avatarUrl: form.avatarUrl || undefined,
      yearsExperience: form.yearsExperience ? Number(form.yearsExperience) : undefined,
      instagramHandle: form.instagramHandle || undefined,
      websiteUrl: form.websiteUrl || undefined,
      isActive,
      danceStyleIds: form.danceStyleIds,
    });
    revalidatePath("/admin/coaches");
    return { success: true, message: "Coach updated." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
