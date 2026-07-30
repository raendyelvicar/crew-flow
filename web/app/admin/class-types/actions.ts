"use server";

import { revalidatePath } from "next/cache";
import { apiClient, ApiError } from "@/lib/api-client";

export async function createClassType(name: string, description: string) {
  try {
    await apiClient.post("/api/v1/class-types", { name, description: description || undefined, isActive: true });
    revalidatePath("/admin/class-types");
    return { success: true, message: "Class type created." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}

export async function updateClassType(id: string, name: string, description: string, isActive: boolean) {
  try {
    await apiClient.put(`/api/v1/class-types/${id}`, { name, description: description || undefined, isActive });
    revalidatePath("/admin/class-types");
    return { success: true, message: "Class type updated." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
