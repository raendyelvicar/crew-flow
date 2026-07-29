import { notFound } from "next/navigation";
import { CmsSections } from "@/components/cms-sections";
import { publicGet } from "@/lib/api-client";

export const dynamic = "force-dynamic";
import type { CmsPage } from "@/lib/types";

export default async function CmsPageRoute({ params }: PageProps<"/[slug]">) {
  const { slug } = await params;
  const page = await publicGet<CmsPage>(`/api/v1/cms/pages/${slug}`);

  if (!page) {
    notFound();
  }

  return (
    <div>
      <div className="px-4 pt-10">
        <h1 className="mx-auto max-w-4xl text-2xl font-bold tracking-tight sm:text-3xl">{page.title}</h1>
      </div>
      <CmsSections sections={page.sections} />
    </div>
  );
}
