"use client";

import type { ReactNode } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { signOut, useSession } from "next-auth/react";
import { LogOut } from "lucide-react";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar";

export type AdminNavItem = { href: string; label: string; icon: ReactNode };
export type AdminNavSection = { label: string; items: AdminNavItem[] };

export function AdminSidebar({ sections, children }: { sections: AdminNavSection[]; children: React.ReactNode }) {
  const pathname = usePathname();
  const { data: session } = useSession();

  return (
    <SidebarProvider>
      <Sidebar collapsible="icon">
        <SidebarHeader>
          <Link href="/admin" className="flex h-8 items-center px-2 font-semibold tracking-tight">
            <span className="truncate group-data-[collapsible=icon]:hidden">Crew Flow</span>
          </Link>
        </SidebarHeader>
        <SidebarContent>
          {sections.map((section) => (
            <SidebarGroup key={section.label}>
              <SidebarGroupLabel>{section.label}</SidebarGroupLabel>
              <SidebarGroupContent>
                <SidebarMenu>
                  {section.items.map((item) => (
                    <SidebarMenuItem key={item.href}>
                      <SidebarMenuButton isActive={pathname === item.href} tooltip={item.label} render={<Link href={item.href} />}>
                        {item.icon}
                        <span>{item.label}</span>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  ))}
                </SidebarMenu>
              </SidebarGroupContent>
            </SidebarGroup>
          ))}
        </SidebarContent>
        <SidebarFooter>
          <SidebarMenu>
            <SidebarMenuItem>
              <SidebarMenuButton tooltip="Sign out" onClick={() => signOut({ callbackUrl: "/" })}>
                <LogOut />
                <span>{session?.user?.name ?? "Sign out"}</span>
              </SidebarMenuButton>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarFooter>
      </Sidebar>
      <SidebarInset>
        <header className="flex h-14 items-center gap-3 border-b px-4">
          <SidebarTrigger />
          <span className="font-semibold">Back office</span>
        </header>
        <main className="flex-1 px-4 py-6 sm:py-8">
          <div className="mx-auto max-w-6xl">{children}</div>
        </main>
      </SidebarInset>
    </SidebarProvider>
  );
}
