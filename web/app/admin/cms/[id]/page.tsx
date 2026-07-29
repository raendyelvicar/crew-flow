import { Badge } from "@/components/ui/badge";
import { PublishToggle } from "@/components/publish-toggle";
import { SectionEditor } from "@/components/section-editor";
import { apiClient } from "@/lib/api-client";
import type { CmsPage } from "@/lib/types";

export default async function CmsPageEditor({ params }: PageProps<"/admin/cms/[id]">) {
  const { id } = await params;
  const page = await apiClient.get<CmsPage>(`/api/v1/cms/pages/by-id/${id}`);

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{page.title}</h1>
          <p className="text-sm text-muted-foreground">/{page.slug}</p>
        </div>
        <div className="flex items-center gap-3">
          <Badge variant={page.isPublished ? "default" : "secondary"}>{page.isPublished ? "Published" : "Draft"}</Badge>
          <PublishToggle pageId={page.id} isPublished={page.isPublished} />
        </div>
      </div>

      <SectionEditor pageId={page.id} sections={page.sections} />
    </div>
  );
}
