import { redirect } from "next/navigation";
import { auth } from "@/lib/auth";
import { AppNav } from "@/components/nav";
import { resolveHomeRoute } from "@/lib/roles";

const links = [{ href: "/coach", label: "My classes" }];

export default async function CoachLayout({ children }: { children: React.ReactNode }) {
  const session = await auth();

  if (!session) {
    redirect("/login?callbackUrl=/coach");
  }

  const roles = session.roles ?? [];
  if (!roles.includes("Admin") && !roles.includes("Coach")) {
    redirect(resolveHomeRoute(roles));
  }

  return (
    <div className="flex min-h-screen flex-col">
      <AppNav brandHref="/coach" links={links} />
      <main className="flex-1 px-4 py-6 sm:py-8">
        <div className="mx-auto max-w-5xl">{children}</div>
      </main>
    </div>
  );
}
