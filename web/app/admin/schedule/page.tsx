import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { CreateActivityDialog } from "@/components/create-activity-dialog";
import { CreateClassScheduleDialog } from "@/components/create-class-schedule-dialog";
import { apiClient } from "@/lib/api-client";
import type { Activity, ClassOccurrence, ClassSchedule } from "@/lib/types";

export default async function SchedulePage() {
  const from = new Date();
  const to = new Date();
  to.setDate(to.getDate() + 14);

  const [activities, schedules, occurrences] = await Promise.all([
    apiClient.get<Activity[]>("/api/v1/activities?activeOnly=true"),
    apiClient.get<ClassSchedule[]>("/api/v1/class-schedules"),
    apiClient.get<ClassOccurrence[]>(
      `/api/v1/class-occurrences?fromUtc=${from.toISOString()}&toUtc=${to.toISOString()}`
    ),
  ]);

  return (
    <div className="space-y-8">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Schedule</h1>
          <p className="text-sm text-muted-foreground">Activities, recurring classes, and the next 2 weeks.</p>
        </div>
        <div className="flex gap-2">
          <CreateActivityDialog />
          <CreateClassScheduleDialog activities={activities} />
        </div>
      </div>

      <div>
        <h2 className="mb-3 text-lg font-semibold">Recurring classes</h2>
        {schedules.length === 0 ? (
          <p className="text-sm text-muted-foreground">No recurring classes set up yet.</p>
        ) : (
          <div className="grid gap-3 sm:grid-cols-2">
            {schedules.map((schedule) => (
              <Card key={schedule.id}>
                <CardContent className="pt-6 text-sm">
                  <p className="font-medium">{schedule.activityName}</p>
                  <p className="text-muted-foreground">
                    {schedule.dayOfWeek}s at {schedule.startTimeLocal.slice(0, 5)} ({schedule.durationMinutes} min)
                  </p>
                  <p className="text-muted-foreground">Instructor: {schedule.instructorName}</p>
                  <p className="text-muted-foreground">Capacity: {schedule.capacity}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>

      <div>
        <h2 className="mb-3 text-lg font-semibold">Upcoming occurrences</h2>
        {occurrences.length === 0 ? (
          <p className="text-sm text-muted-foreground">Nothing scheduled in the next 2 weeks.</p>
        ) : (
          <div className="space-y-2">
            {occurrences.map((occurrence) => (
              <Link
                key={occurrence.id}
                href={`/admin/schedule/occurrences/${occurrence.id}`}
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
                    })}{" "}
                    with {occurrence.instructorName}
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
    </div>
  );
}
