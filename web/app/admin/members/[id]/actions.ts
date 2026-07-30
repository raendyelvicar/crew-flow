"use server";

import { revalidatePath } from "next/cache";
import { apiClient, ApiError } from "@/lib/api-client";
import type { MemberStatus, SkillLevel } from "@/lib/types";

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

export type MemberProfileFormInput = {
  phone: string;
  dateOfBirth: string;
  emergencyContactName: string;
  emergencyContactPhone: string;
  bio: string;
  avatarUrl: string;
  instagramHandle: string;
  tikTokHandle: string;
  websiteUrl: string;
  isProfilePublic: boolean;
};

export async function updateMemberProfile(memberId: string, form: MemberProfileFormInput) {
  try {
    await apiClient.patch(`/api/v1/members/${memberId}`, {
      phone: form.phone || undefined,
      dateOfBirth: form.dateOfBirth || undefined,
      emergencyContactName: form.emergencyContactName || undefined,
      emergencyContactPhone: form.emergencyContactPhone || undefined,
      bio: form.bio || undefined,
      avatarUrl: form.avatarUrl || undefined,
      instagramHandle: form.instagramHandle || undefined,
      tikTokHandle: form.tikTokHandle || undefined,
      websiteUrl: form.websiteUrl || undefined,
      isProfilePublic: form.isProfilePublic,
    });
    revalidatePath(`/admin/members/${memberId}`);
    return { success: true, message: "Profile updated." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}

export async function setMemberDanceStyles(memberId: string, styles: { danceStyleId: string; skillLevel: SkillLevel }[]) {
  try {
    await apiClient.put(`/api/v1/members/${memberId}/dance-styles`, { styles });
    revalidatePath(`/admin/members/${memberId}`);
    return { success: true, message: "Dance styles updated." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
