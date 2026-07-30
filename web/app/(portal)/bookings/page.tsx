import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { CancelBookingButton } from "@/components/booking-buttons";
import { BookingQrDialog } from "@/components/booking-qr-dialog";
import { apiClient } from "@/lib/api-client";
import type { MyBooking } from "@/lib/types";

function statusVariant(status: MyBooking["status"]) {
  switch (status) {
    case "Booked":
    case "Attended":
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

function BookingRow({ booking, showQr }: { booking: MyBooking; showQr: boolean }) {
  const canCancel = booking.status === "Booked" || booking.status === "Waitlisted";
  return (
    <Card>
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
          {showQr && booking.status === "Booked" && (
            <BookingQrDialog bookingId={booking.id} activityName={booking.activityName} />
          )}
          {canCancel && <CancelBookingButton bookingId={booking.id} />}
        </div>
      </CardContent>
    </Card>
  );
}

export default async function BookingsPage() {
  const bookings = await apiClient.get<MyBooking[]>("/api/v1/bookings/me");
  const now = new Date();

  const upcoming = bookings
    .filter((b) => (b.status === "Booked" || b.status === "Waitlisted") && new Date(b.startAtUtc) >= now)
    .sort((a, b) => new Date(a.startAtUtc).getTime() - new Date(b.startAtUtc).getTime());

  const history = bookings
    .filter((b) => !upcoming.includes(b))
    .sort((a, b) => new Date(b.startAtUtc).getTime() - new Date(a.startAtUtc).getTime());

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">My bookings</h1>
        <p className="text-sm text-muted-foreground">Your upcoming reservations and class history.</p>
      </div>

      <div>
        <h2 className="mb-3 text-lg font-semibold">Upcoming</h2>
        {upcoming.length === 0 ? (
          <p className="text-sm text-muted-foreground">Nothing booked yet - head to Classes to reserve a spot.</p>
        ) : (
          <div className="space-y-3">
            {upcoming.map((booking) => (
              <BookingRow key={booking.id} booking={booking} showQr />
            ))}
          </div>
        )}
      </div>

      <div>
        <h2 className="mb-3 text-lg font-semibold">History</h2>
        {history.length === 0 ? (
          <p className="text-sm text-muted-foreground">No past classes yet.</p>
        ) : (
          <div className="space-y-3">
            {history.map((booking) => (
              <BookingRow key={booking.id} booking={booking} showQr={false} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
