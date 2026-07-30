import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { CoachDialog } from "@/components/coach-dialog";
import { apiClient } from "@/lib/api-client";
import type { DanceStyle, Instructor } from "@/lib/types";

export default async function CoachesPage() {
  const [coaches, danceStyles] = await Promise.all([
    apiClient.get<Instructor[]>("/api/v1/instructors?activeOnly=false"),
    apiClient.get<DanceStyle[]>("/api/v1/dance-styles?activeOnly=true"),
  ]);

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Coaches</h1>
          <p className="text-sm text-muted-foreground">{coaches.length} total</p>
        </div>
        <CoachDialog danceStyles={danceStyles} />
      </div>

      <div className="space-y-2">
        {coaches.map((coach) => (
          <div key={coach.id} className="flex items-center justify-between rounded-lg border p-4">
            <div className="flex items-center gap-3">
              <Avatar>
                <AvatarFallback>
                  {coach.firstName[0]}
                  {coach.lastName[0]}
                </AvatarFallback>
              </Avatar>
              <div>
                <div className="flex items-center gap-2">
                  <p className="font-medium">
                    {coach.firstName} {coach.lastName}
                  </p>
                  <Badge variant={coach.isActive ? "default" : "secondary"}>{coach.isActive ? "Active" : "Inactive"}</Badge>
                </div>
                {coach.danceStyles.length > 0 && (
                  <p className="text-sm text-muted-foreground">
                    {coach.danceStyles.map((s) => s.danceStyleName).join(", ")}
                  </p>
                )}
              </div>
            </div>
            <CoachDialog coach={coach} danceStyles={danceStyles} />
          </div>
        ))}
      </div>
    </div>
  );
}
