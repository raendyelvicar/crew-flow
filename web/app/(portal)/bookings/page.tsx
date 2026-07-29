import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { CancelBookingButton } from "@/components/booking-buttons";
import { apiClient } from "@/lib/api-client";
import type { MyBooking } from "@/lib/types";

function statusVariant(status: MyBooking["status"]) {
  switch (status) {
    case "Booked":
      return "default" as const;
    case "Waitlisted":
      return "secondary" as const;
    case "Cancelled":
    case "NoShow":
      return "outline" as const;
    default:
      return "secondary" as const;
  }
}

export default async function BookingsPage() {
  const bookings = await apiClient.get<MyBooking[]>("/api/v1/bookings/me");
  const sorted = [...bookings].sort((a, b) => new Date(b.startAtUtc).getTime() - new Date(a.startAtUtc).getTime());

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">My bookings</h1>
        <p className="text-sm text-muted-foreground">Your class history and upcoming reservations.</p>
      </div>

      {sorted.length === 0 ? (
        <p className="text-sm text-muted-foreground">You haven&apos;t booked any classes yet.</p>
      ) : (
        <div className="space-y-3">
          {sorted.map((booking) => {
            const canCancel = booking.status === "Booked" || booking.status === "Waitlisted";
            return (
              <Card key={booking.id}>
                <CardContent className="flex flex-col gap-2 pt-6 sm:flex-row sm:items-center sm:justify-between">
                  <div>
                    <p className="font-medium">{booking.activityName}</p>
                    <p className="text-sm text-muted-foreground">{new Date(booking.startAtUtc).toLocaleString()}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    <Badge variant={statusVariant(booking.status)}>
                      {booking.status}
                      {booking.status === "Waitlisted" && booking.waitlistPosition ? ` #${booking.waitlistPosition}` : ""}
                    </Badge>
                    {canCancel && <CancelBookingButton bookingId={booking.id} />}
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
