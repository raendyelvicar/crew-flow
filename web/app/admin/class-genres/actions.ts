"use server";

import { revalidatePath } from "next/cache";
import { apiClient, ApiError } from "@/lib/api-client";

export async function createClassGenre(name: string) {
  try {
    await apiClient.post("/api/v1/dance-styles", { name, isActive: true });
    revalidatePath("/admin/class-genres");
    return { success: true, message: "Class genre created." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}

export async function updateClassGenre(id: string, name: string, isActive: boolean) {
  try {
    await apiClient.put(`/api/v1/dance-styles/${id}`, { name, isActive });
    revalidatePath("/admin/class-genres");
    return { success: true, message: "Class genre updated." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
