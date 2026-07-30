import Link from "next/link";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { auth } from "@/lib/auth";

export default async function AdminHomePage() {
  const session = await auth();
  const roles = session?.roles ?? [];
  const isAdmin = roles.includes("Admin");
  const isFinance = isAdmin || roles.includes("Finance");
  const isOperational = isAdmin || roles.includes("Operational");

  const sections = [
    isOperational && { href: "/admin/members", title: "Members", description: "Manage member profiles and status." },
    isOperational && { href: "/admin/schedule", title: "Schedule", description: "Calendar, activities, and class schedules." },
    isOperational && { href: "/admin/coaches", title: "Coaches", description: "Manage instructor profiles." },
    isOperational && { href: "/admin/class-genres", title: "Class Genres", description: "K-Pop, Contemporary, Hip-Hop, and more." },
    isOperational && { href: "/admin/class-types", title: "Class Types", description: "Regular, Open, Kids, ICM Course." },
    isFinance && { href: "/admin/cashflow", title: "Cashflow", description: "Track income, refunds, and reconciliation." },
    isFinance && { href: "/admin/packages", title: "Packages", description: "Membership plans and drop-in credit packs." },
  ].filter(Boolean) as { href: string; title: string; description: string }[];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Back office</h1>
        <div className="mt-1 flex gap-1.5">
          {roles.map((role) => (
            <Badge key={role} variant="secondary">
              {role}
            </Badge>
          ))}
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {sections.map((section) => (
          <Link key={section.href} href={section.href}>
            <Card className="h-full transition-colors hover:bg-accent/50">
              <CardHeader>
                <CardTitle className="text-base">{section.title}</CardTitle>
                <CardDescription>{section.description}</CardDescription>
              </CardHeader>
              <CardContent />
            </Card>
          </Link>
        ))}
      </div>
    </div>
  );
}
