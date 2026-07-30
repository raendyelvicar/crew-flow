import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { apiClient } from "@/lib/api-client";
import type { ClassOccurrence } from "@/lib/types";

export default async function CoachDashboardPage() {
  const from = new Date();
  const to = new Date();
  to.setDate(to.getDate() + 14);

  const occurrences = await apiClient.get<ClassOccurrence[]>(
    `/api/v1/class-occurrences/mine?fromUtc=${from.toISOString()}&toUtc=${to.toISOString()}`
  );

  const upcoming = occurrences
    .filter((o) => o.status === "Scheduled")
    .sort((a, b) => new Date(a.startAtUtc).getTime() - new Date(b.startAtUtc).getTime());

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">My classes</h1>
        <p className="text-sm text-muted-foreground">Your upcoming classes over the next two weeks.</p>
      </div>

      {upcoming.length === 0 ? (
        <p className="text-sm text-muted-foreground">No upcoming classes assigned to you.</p>
      ) : (
        <div className="space-y-2">
          {upcoming.map((occurrence) => (
            <Link
              key={occurrence.id}
              href={`/coach/classes/${occurrence.id}`}
              className="flex items-center justify-between rounded-lg border p-4 text-sm hover:bg-accent/50"
            >
              <div>
                <p className="font-medium">{occurrence.activityName}</p>
                <p className="text-muted-foreground">
                  {new Date(occurrence.startAtUtc).toLocaleString(undefined, {
                    weekday: "short",
                    month: "short",
                    day: "numeric",
                    hour: "numeric",
                    minute: "2-digit",
                  })}
                </p>
              </div>
              <Badge variant={occurrence.bookedCount >= occurrence.capacity ? "secondary" : "default"}>
                {occurrence.bookedCount}/{occurrence.capacity}
              </Badge>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
