import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { ClassOccurrence } from "@/lib/types";

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString(undefined, { hour: "numeric", minute: "2-digit" });
}

function OccurrenceCard({ occurrence }: { occurrence: ClassOccurrence }) {
  const isFull = occurrence.bookedCount >= occurrence.capacity;
  return (
    <Link
      href={`/admin/schedule/occurrences/${occurrence.id}`}
      className="block rounded-md border bg-card p-2 text-xs transition-colors hover:bg-accent/50"
    >
      <p className="font-medium">{formatTime(occurrence.startAtUtc)}</p>
      <p className="truncate">{occurrence.activityName}</p>
      <p className="truncate text-muted-foreground">{occurrence.instructorName}</p>
      <Badge variant={isFull ? "secondary" : "default"} className="mt-1">
        {occurrence.bookedCount}/{occurrence.capacity}
      </Badge>
    </Link>
  );
}

export function ScheduleCalendar({ weekStart, occurrences }: { weekStart: Date; occurrences: ClassOccurrence[] }) {
  const days = Array.from({ length: 7 }, (_, i) => {
    const date = new Date(weekStart);
    date.setDate(date.getDate() + i);
    return date;
  });

  const byDay = days.map((date) => {
    const dayOccurrences = occurrences
      .filter((o) => {
        const start = new Date(o.startAtUtc);
        return (
          start.getFullYear() === date.getFullYear() &&
          start.getMonth() === date.getMonth() &&
          start.getDate() === date.getDate()
        );
      })
      .sort((a, b) => new Date(a.startAtUtc).getTime() - new Date(b.startAtUtc).getTime());
    return { date, occurrences: dayOccurrences };
  });

  const today = new Date();
  const isToday = (date: Date) =>
    date.getFullYear() === today.getFullYear() && date.getMonth() === today.getMonth() && date.getDate() === today.getDate();

  return (
    <>
      {/* Mobile: stacked day sections */}
      <div className="space-y-4 sm:hidden">
        {byDay.map(({ date, occurrences: dayOccurrences }) => (
          <div key={date.toISOString()}>
            <h3 className={cn("mb-2 text-sm font-semibold", isToday(date) && "text-primary")}>
              {date.toLocaleDateString(undefined, { weekday: "long", month: "short", day: "numeric" })}
            </h3>
            {dayOccurrences.length === 0 ? (
              <p className="text-sm text-muted-foreground">No classes.</p>
            ) : (
              <div className="space-y-2">
                {dayOccurrences.map((o) => (
                  <OccurrenceCard key={o.id} occurrence={o} />
                ))}
              </div>
            )}
          </div>
        ))}
      </div>

      {/* Desktop: 7-column grid */}
      <div className="hidden grid-cols-7 gap-2 sm:grid">
        {byDay.map(({ date, occurrences: dayOccurrences }) => (
          <div key={date.toISOString()} className="space-y-2">
            <h3
              className={cn(
                "rounded-md px-2 py-1 text-center text-sm font-semibold",
                isToday(date) ? "bg-primary text-primary-foreground" : "bg-muted"
              )}
            >
              {date.toLocaleDateString(undefined, { weekday: "short" })}
              <span className="block text-xs font-normal">{date.toLocaleDateString(undefined, { month: "short", day: "numeric" })}</span>
            </h3>
            <div className="space-y-2">
              {dayOccurrences.map((o) => (
                <OccurrenceCard key={o.id} occurrence={o} />
              ))}
            </div>
          </div>
        ))}
      </div>
    </>
  );
}
