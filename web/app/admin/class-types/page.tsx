import { Badge } from "@/components/ui/badge";
import { ClassTypeDialog } from "@/components/class-type-dialog";
import { apiClient } from "@/lib/api-client";
import type { ClassType } from "@/lib/types";

export default async function ClassTypesPage() {
  const types = await apiClient.get<ClassType[]>("/api/v1/class-types?activeOnly=false");

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Class Types</h1>
          <p className="text-sm text-muted-foreground">Regular, Open, Kids, ICM Course, and any others you define.</p>
        </div>
        <ClassTypeDialog />
      </div>

      <div className="space-y-2">
        {types.map((type) => (
          <div key={type.id} className="flex items-center justify-between rounded-lg border p-4">
            <div>
              <div className="flex items-center gap-3">
                <p className="font-medium">{type.name}</p>
                <Badge variant={type.isActive ? "default" : "secondary"}>{type.isActive ? "Active" : "Inactive"}</Badge>
              </div>
              {type.description && <p className="text-sm text-muted-foreground">{type.description}</p>}
            </div>
            <ClassTypeDialog classType={type} />
          </div>
        ))}
      </div>
    </div>
  );
}
