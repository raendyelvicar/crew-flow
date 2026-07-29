import Link from "next/link";
import { buttonVariants } from "@/components/ui/button";
import { CmsSections } from "@/components/cms-sections";

// Fetches from the .NET API at request time - opting out of static generation
// avoids coupling the web image's build step to the API being reachable.
export const dynamic = "force-dynamic";
import { publicGet } from "@/lib/api-client";
import type { CmsPage, MembershipPlan } from "@/lib/types";

export default async function HomePage() {
  const [page, plans] = await Promise.all([
    publicGet<CmsPage>("/api/v1/cms/pages/home"),
    publicGet<MembershipPlan[]>("/api/v1/membership-plans?activeOnly=true").then((res) => res ?? []),
  ]);

  if (page) {
    return <CmsSections sections={page.sections} plans={plans} />;
  }

  return (
    <section className="px-4 py-16 sm:py-24">
      <div className="mx-auto flex max-w-3xl flex-col items-center gap-6 text-center">
        <h1 className="text-3xl font-bold tracking-tight sm:text-5xl">Welcome to Crew Flow</h1>
        <p className="max-w-xl text-base text-muted-foreground sm:text-lg">
          A dance studio and community - browse classes, join the crew, and grow your practice.
        </p>
        <div className="flex flex-col gap-3 sm:flex-row">
          <Link href="/register" className={buttonVariants({ size: "lg" })}>
            Join now
          </Link>
          <Link href="/pricing" className={buttonVariants({ size: "lg", variant: "outline" })}>
            View membership
          </Link>
        </div>
        <p className="text-xs text-muted-foreground">
          No landing page content yet - an admin can publish one from the CMS.
        </p>
      </div>
    </section>
  );
}
