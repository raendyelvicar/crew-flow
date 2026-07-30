import { redirect } from "next/navigation";
import { auth } from "@/lib/auth";
import { AppNav } from "@/components/nav";
import { resolveHomeRoute } from "@/lib/roles";

const links = [
  { href: "/dashboard", label: "Dashboard" },
  { href: "/classes", label: "Classes" },
  { href: "/bookings", label: "My bookings" },
  { href: "/membership", label: "Membership" },
];

export default async function PortalLayout({ children }: { children: React.ReactNode }) {
  const session = await auth();

  if (!session) {
    redirect("/login?callbackUrl=/dashboard");
  }

  if (!session.roles?.includes("Member")) {
    redirect(resolveHomeRoute(session.roles));
  }

  return (
    <div className="flex min-h-screen flex-col">
      <AppNav brandHref="/dashboard" links={links} />
      <main className="flex-1 px-4 py-6 sm:py-8">
        <div className="mx-auto max-w-5xl">{children}</div>
      </main>
    </div>
  );
}
