import { redirect } from "next/navigation";
import { auth } from "@/lib/auth";
import { AppNav, type NavLink } from "@/components/nav";

export default async function AdminLayout({ children }: { children: React.ReactNode }) {
  const session = await auth();

  if (!session) {
    redirect("/login?callbackUrl=/admin");
  }

  const roles = session.roles ?? [];
  const isAdmin = roles.includes("Admin");
  const isFinance = isAdmin || roles.includes("Finance");
  const isOperational = isAdmin || roles.includes("Operational");

  if (!isFinance && !isOperational) {
    redirect("/dashboard");
  }

  const links: NavLink[] = [];
  if (isOperational) {
    links.push(
      { href: "/admin/members", label: "Members" },
      { href: "/admin/schedule", label: "Schedule" },
      { href: "/admin/coaches", label: "Coaches" },
      { href: "/admin/class-genres", label: "Class Genres" },
      { href: "/admin/class-types", label: "Class Types" }
    );
  }
  if (isFinance) {
    links.push({ href: "/admin/cashflow", label: "Cashflow" }, { href: "/admin/packages", label: "Packages" });
  }

  return (
    <div className="flex min-h-screen flex-col">
      <AppNav brandHref="/admin" links={links} />
      <main className="flex-1 px-4 py-6 sm:py-8">
        <div className="mx-auto max-w-6xl">{children}</div>
      </main>
    </div>
  );
}
