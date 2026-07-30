import { RosterTable } from "@/components/roster-table";
import { QrCheckIn } from "@/components/qr-check-in";
import { apiClient } from "@/lib/api-client";
import type { RosterEntry } from "@/lib/types";
import { markAttendance } from "./actions";

export default async function CoachClassRosterPage({ params }: PageProps<"/coach/classes/[occurrenceId]">) {
  const { occurrenceId } = await params;
  const roster = await apiClient.get<RosterEntry[]>(`/api/v1/class-occurrences/${occurrenceId}/roster`);
  const checkIn = markAttendance.bind(null, occurrenceId, "Attended");
  const markAttendanceAction = markAttendance.bind(null, occurrenceId);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Class roster</h1>
        <p className="text-sm text-muted-foreground">{roster.length} booking(s)</p>
      </div>

      {roster.length === 0 ? (
        <p className="text-sm text-muted-foreground">No one has booked this class yet.</p>
      ) : (
        <>
          <QrCheckIn roster={roster} onCheckIn={checkIn} />
          <RosterTable roster={roster} onMarkAttendance={markAttendanceAction} />
        </>
      )}
    </div>
  );
}
