import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { BookButton } from "@/components/booking-buttons";
import { publicGet } from "@/lib/api-client";
import type { ClassOccurrence } from "@/lib/types";

export default async function ClassesPage() {
  const from = new Date();
  const to = new Date();
  to.setDate(to.getDate() + 60);

  const occurrences =
    (await publicGet<ClassOccurrence[]>(
      `/api/v1/class-occurrences?fromUtc=${from.toISOString()}&toUtc=${to.toISOString()}`,
      0
    )) ?? [];

  const upcoming = occurrences
    .filter((o) => o.status === "Scheduled")
    .sort((a, b) => new Date(a.startAtUtc).getTime() - new Date(b.startAtUtc).getTime());

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Classes</h1>
        <p className="text-sm text-muted-foreground">Book a spot - class full? You&apos;ll be added to the waitlist.</p>
      </div>

      {upcoming.length === 0 ? (
        <p className="text-sm text-muted-foreground">No upcoming classes scheduled yet.</p>
      ) : (
        <div className="grid gap-3 sm:grid-cols-2">
          {upcoming.map((occurrence) => {
            const isFull = occurrence.bookedCount >= occurrence.capacity;
            return (
              <Card key={occurrence.id}>
                <CardContent className="flex flex-col gap-2 pt-6">
                  <div className="flex items-start justify-between gap-2">
                    <div>
                      <p className="font-medium">{occurrence.activityName}</p>
                      <p className="text-sm text-muted-foreground">
                        {new Date(occurrence.startAtUtc).toLocaleString(undefined, {
                          weekday: "short",
                          month: "short",
                          day: "numeric",
                          hour: "numeric",
                          minute: "2-digit",
                        })}
                      </p>
                      <p className="text-sm text-muted-foreground">with {occurrence.instructorName}</p>
                    </div>
                    <Badge variant={isFull ? "secondary" : "default"}>
                      {occurrence.bookedCount}/{occurrence.capacity}
                      {isFull ? " full" : ""}
                    </Badge>
                  </div>
                  <div className="flex items-center justify-between pt-2">
                    {isFull && occurrence.waitlistCount > 0 && (
                      <span className="text-xs text-muted-foreground">{occurrence.waitlistCount} waitlisted</span>
                    )}
                    <div className="ml-auto">
                      <BookButton classOccurrenceId={occurrence.id} />
                    </div>
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
