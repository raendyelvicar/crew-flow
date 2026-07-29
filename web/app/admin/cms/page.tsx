import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { CreatePageDialog } from "@/components/create-page-dialog";
import { PublishToggle } from "@/components/publish-toggle";
import { apiClient } from "@/lib/api-client";
import type { CmsPage } from "@/lib/types";

export default async function CmsPagesListPage() {
  const pages = await apiClient.get<CmsPage[]>("/api/v1/cms/pages");

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">CMS pages</h1>
          <p className="text-sm text-muted-foreground">Edit the public marketing site.</p>
        </div>
        <CreatePageDialog />
      </div>

      <div className="space-y-2">
        {pages.map((page) => (
          <div key={page.id} className="flex items-center justify-between rounded-lg border p-4">
            <div>
              <Link href={`/admin/cms/${page.id}`} className="font-medium hover:underline">
                {page.title}
              </Link>
              <p className="text-sm text-muted-foreground">/{page.slug}</p>
            </div>
            <div className="flex items-center gap-3">
              <Badge variant={page.isPublished ? "default" : "secondary"}>
                {page.isPublished ? "Published" : "Draft"}
              </Badge>
              <PublishToggle pageId={page.id} isPublished={page.isPublished} />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
