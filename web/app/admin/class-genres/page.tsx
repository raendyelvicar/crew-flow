import { Badge } from "@/components/ui/badge";
import { ClassGenreDialog } from "@/components/class-genre-dialog";
import { apiClient } from "@/lib/api-client";
import type { DanceStyle } from "@/lib/types";

export default async function ClassGenresPage() {
  const genres = await apiClient.get<DanceStyle[]>("/api/v1/dance-styles?activeOnly=false");

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Class Genres</h1>
          <p className="text-sm text-muted-foreground">
            The dance styles used to classify classes and shown on member community profiles.
          </p>
        </div>
        <ClassGenreDialog />
      </div>

      <div className="space-y-2">
        {genres.map((genre) => (
          <div key={genre.id} className="flex items-center justify-between rounded-lg border p-4">
            <div className="flex items-center gap-3">
              <p className="font-medium">{genre.name}</p>
              <Badge variant={genre.isActive ? "default" : "secondary"}>{genre.isActive ? "Active" : "Inactive"}</Badge>
            </div>
            <ClassGenreDialog genre={genre} />
          </div>
        ))}
      </div>
    </div>
  );
}
