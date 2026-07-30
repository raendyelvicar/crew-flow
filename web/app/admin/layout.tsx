import { redirect } from "next/navigation";
import { CalendarDays, DollarSign, Music2, Package, Tag, UserCog, Users } from "lucide-react";
import { auth } from "@/lib/auth";
import { AdminSidebar, type AdminNavSection } from "@/components/admin-sidebar";

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

  const sections: AdminNavSection[] = [];
  if (isOperational) {
    sections.push({
      label: "Operations",
      items: [
        { href: "/admin/members", label: "Members", icon: <Users /> },
        { href: "/admin/schedule", label: "Schedule", icon: <CalendarDays /> },
        { href: "/admin/coaches", label: "Coaches", icon: <UserCog /> },
        { href: "/admin/class-genres", label: "Class Genres", icon: <Music2 /> },
        { href: "/admin/class-types", label: "Class Types", icon: <Tag /> },
      ],
    });
  }
  if (isFinance) {
    sections.push({
      label: "Finance",
      items: [
        { href: "/admin/cashflow", label: "Cashflow", icon: <DollarSign /> },
        { href: "/admin/packages", label: "Packages", icon: <Package /> },
      ],
    });
  }

  return <AdminSidebar sections={sections}>{children}</AdminSidebar>;
}
