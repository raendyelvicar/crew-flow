import { RosterTable } from "@/components/roster-table";
import { apiClient } from "@/lib/api-client";
import type { RosterEntry } from "@/lib/types";

export default async function OccurrenceRosterPage({ params }: PageProps<"/admin/schedule/occurrences/[id]">) {
  const { id } = await params;
  const roster = await apiClient.get<RosterEntry[]>(`/api/v1/class-occurrences/${id}/roster`);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Roster</h1>
        <p className="text-sm text-muted-foreground">{roster.length} booking(s)</p>
      </div>

      {roster.length === 0 ? (
        <p className="text-sm text-muted-foreground">No one has booked this class yet.</p>
      ) : (
        <RosterTable occurrenceId={id} roster={roster} />
      )}
    </div>
  );
}
