import Link from "next/link";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { buttonVariants } from "@/components/ui/button";
import { apiClient, ApiError } from "@/lib/api-client";
import type { CreditPackPurchase, MyBooking, Subscription } from "@/lib/types";

async function safeGet<T>(path: string, fallback: T): Promise<T> {
  try {
    return await apiClient.get<T>(path);
  } catch (err) {
    if (err instanceof ApiError && (err.status === 404 || err.status === 409)) return fallback;
    throw err;
  }
}

export default async function DashboardPage() {
  const [bookings, subscription, purchases] = await Promise.all([
    safeGet<MyBooking[]>("/api/v1/bookings/me", []),
    safeGet<Subscription | null>("/api/v1/subscriptions/me", null),
    safeGet<CreditPackPurchase[]>("/api/v1/credit-packs/purchases/me", []),
  ]);

  const upcoming = bookings
    .filter((b) => (b.status === "Booked" || b.status === "Waitlisted") && new Date(b.startAtUtc) > new Date())
    .sort((a, b) => new Date(a.startAtUtc).getTime() - new Date(b.startAtUtc).getTime())
    .slice(0, 5);

  const activeCredits = purchases.filter((p) => p.status === "Active").reduce((sum, p) => sum + p.creditsRemaining, 0);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-sm text-muted-foreground">Your next classes and membership status.</p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Membership</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {subscription ? (
              <>
                <p className="font-medium">{subscription.planName}</p>
                <Badge variant={subscription.status === "Active" ? "default" : "secondary"}>{subscription.status}</Badge>
              </>
            ) : (
              <p className="text-sm text-muted-foreground">No active subscription.</p>
            )}
            <p className="text-sm text-muted-foreground">{activeCredits} drop-in credit(s) remaining</p>
            <Link href="/membership" className={buttonVariants({ variant: "link", className: "h-auto p-0" })}>
              Manage membership &rarr;
            </Link>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Upcoming classes</CardTitle>
            <CardDescription>{upcoming.length === 0 ? "Nothing booked yet" : `${upcoming.length} upcoming`}</CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {upcoming.map((booking) => (
              <div key={booking.id} className="flex items-center justify-between text-sm">
                <div>
                  <p className="font-medium">{booking.activityName}</p>
                  <p className="text-muted-foreground">{new Date(booking.startAtUtc).toLocaleString()}</p>
                </div>
                <Badge variant={booking.status === "Booked" ? "default" : "secondary"}>{booking.status}</Badge>
              </div>
            ))}
            <Link href="/classes" className={buttonVariants({ variant: "link", className: "h-auto p-0" })}>
              Browse classes &rarr;
            </Link>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
