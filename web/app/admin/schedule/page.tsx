import Link from "next/link";
import { buttonVariants } from "@/components/ui/button";
import { ActivityDialog } from "@/components/create-activity-dialog";
import { CreateClassScheduleDialog } from "@/components/create-class-schedule-dialog";
import { ScheduleCalendar } from "@/components/schedule-calendar";
import { apiClient } from "@/lib/api-client";
import type { Activity, ClassSchedule, ClassOccurrence, ClassType, DanceStyle, Instructor } from "@/lib/types";

function startOfWeek(date: Date): Date {
  const d = new Date(date);
  const day = d.getDay();
  const diff = day === 0 ? -6 : 1 - day; // Monday as the first day
  d.setDate(d.getDate() + diff);
  d.setHours(0, 0, 0, 0);
  return d;
}

export default async function SchedulePage({ searchParams }: PageProps<"/admin/schedule">) {
  const params = await searchParams;
  const weekParam = typeof params.week === "string" ? params.week : undefined;
  const weekStart = startOfWeek(weekParam ? new Date(weekParam) : new Date());
  const weekEnd = new Date(weekStart);
  weekEnd.setDate(weekEnd.getDate() + 7);

  const prevWeek = new Date(weekStart);
  prevWeek.setDate(prevWeek.getDate() - 7);
  const nextWeek = new Date(weekStart);
  nextWeek.setDate(nextWeek.getDate() + 7);

  const [activities, schedules, occurrences, danceStyles, classTypes, instructors] = await Promise.all([
    apiClient.get<Activity[]>("/api/v1/activities?activeOnly=true"),
    apiClient.get<ClassSchedule[]>("/api/v1/class-schedules"),
    apiClient.get<ClassOccurrence[]>(
      `/api/v1/class-occurrences?fromUtc=${weekStart.toISOString()}&toUtc=${weekEnd.toISOString()}`
    ),
    apiClient.get<DanceStyle[]>("/api/v1/dance-styles?activeOnly=true"),
    apiClient.get<ClassType[]>("/api/v1/class-types?activeOnly=true"),
    apiClient.get<Instructor[]>("/api/v1/instructors?activeOnly=true"),
  ]);

  return (
    <div className="space-y-8">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Schedule</h1>
          <p className="text-sm text-muted-foreground">Weekly calendar, activities, and recurring classes.</p>
        </div>
        <div className="flex gap-2">
          <ActivityDialog danceStyles={danceStyles} classTypes={classTypes} />
          <CreateClassScheduleDialog activities={activities} instructors={instructors} />
        </div>
      </div>

      <div>
        <div className="mb-3 flex items-center justify-between">
          <Link
            href={`/admin/schedule?week=${prevWeek.toISOString().slice(0, 10)}`}
            className={buttonVariants({ variant: "outline", size: "sm" })}
          >
            &larr; Previous
          </Link>
          <p className="text-sm font-medium">
            {weekStart.toLocaleDateString(undefined, { month: "short", day: "numeric" })} -{" "}
            {new Date(weekEnd.getTime() - 86400000).toLocaleDateString(undefined, { month: "short", day: "numeric" })}
          </p>
          <Link
            href={`/admin/schedule?week=${nextWeek.toISOString().slice(0, 10)}`}
            className={buttonVariants({ variant: "outline", size: "sm" })}
          >
            Next &rarr;
          </Link>
        </div>
        <ScheduleCalendar weekStart={weekStart} occurrences={occurrences} />
      </div>

      <div>
        <h2 className="mb-3 text-lg font-semibold">Activities</h2>
        <div className="grid gap-3 sm:grid-cols-2">
          {activities.map((activity) => (
            <div key={activity.id} className="flex items-center justify-between rounded-lg border p-4 text-sm">
              <div>
                <p className="font-medium">{activity.name}</p>
                <p className="text-muted-foreground">
                  {activity.classGenreName} - {activity.classTypeName}
                </p>
              </div>
              <ActivityDialog activity={activity} danceStyles={danceStyles} classTypes={classTypes} />
            </div>
          ))}
        </div>
      </div>

      <div>
        <h2 className="mb-3 text-lg font-semibold">Recurring classes</h2>
        {schedules.length === 0 ? (
          <p className="text-sm text-muted-foreground">No recurring classes set up yet.</p>
        ) : (
          <div className="grid gap-3 sm:grid-cols-2">
            {schedules.map((schedule) => (
              <div key={schedule.id} className="flex items-center justify-between rounded-lg border p-4 text-sm">
                <div>
                  <p className="font-medium">{schedule.activityName}</p>
                  <p className="text-muted-foreground">
                    {schedule.dayOfWeek}s at {schedule.startTimeLocal.slice(0, 5)} ({schedule.durationMinutes} min)
                  </p>
                  <p className="text-muted-foreground">Instructor: {schedule.instructorName}</p>
                  <p className="text-muted-foreground">Capacity: {schedule.capacity}</p>
                </div>
                <CreateClassScheduleDialog activities={activities} instructors={instructors} schedule={schedule} />
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
