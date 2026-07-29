"use server";

import { revalidatePath } from "next/cache";
import { apiClient, ApiError } from "@/lib/api-client";
import type { SectionType } from "@/lib/types";

export async function createPage(slug: string, title: string) {
  try {
    const page = await apiClient.post<{ id: string }>("/api/v1/cms/pages", { slug, title });
    revalidatePath("/admin/cms");
    return { success: true, message: "Page created.", id: page.id };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}

export async function setPagePublished(id: string, isPublished: boolean) {
  try {
    await apiClient.post(`/api/v1/cms/pages/${id}/publish?isPublished=${isPublished}`);
    revalidatePath("/admin/cms");
    return { success: true, message: isPublished ? "Page published." : "Page unpublished." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}

export async function upsertSection(
  pageId: string,
  sectionId: string | null,
  sortOrder: number,
  sectionType: SectionType,
  contentJson: string
) {
  try {
    JSON.parse(contentJson);
  } catch {
    return { success: false, message: "Content must be valid JSON." };
  }

  try {
    const path = sectionId
      ? `/api/v1/cms/pages/${pageId}/sections/${sectionId}`
      : `/api/v1/cms/pages/${pageId}/sections`;
    await apiClient.put(path, { sortOrder, sectionType, contentJson, isVisible: true });
    revalidatePath(`/admin/cms/${pageId}`);
    return { success: true, message: "Section saved." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}

export async function deleteSection(pageId: string, sectionId: string) {
  try {
    await apiClient.del(`/api/v1/cms/pages/${pageId}/sections/${sectionId}`);
    revalidatePath(`/admin/cms/${pageId}`);
    return { success: true, message: "Section removed." };
  } catch (err) {
    if (err instanceof ApiError) return { success: false, message: err.problem?.detail ?? err.message };
    return { success: false, message: "Something went wrong." };
  }
}
