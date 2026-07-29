import { AppNav } from "@/components/nav";

const links = [
  { href: "/", label: "Home" },
  { href: "/pricing", label: "Membership" },
  { href: "/community", label: "Community" },
];

export default function MarketingLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen flex-col">
      <AppNav brandHref="/" links={links} />
      <main className="flex-1">{children}</main>
      <footer className="border-t px-4 py-8 text-center text-sm text-muted-foreground">
        &copy; {new Date().getFullYear()} Crew Flow. All rights reserved.
      </footer>
    </div>
  );
}
