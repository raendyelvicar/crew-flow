import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { MemberStatusSelect } from "@/components/member-status-select";
import { apiClient } from "@/lib/api-client";
import type { Member } from "@/lib/types";

export default async function MemberDetailPage({ params }: PageProps<"/admin/members/[id]">) {
  const { id } = await params;
  const member = await apiClient.get<Member>(`/api/v1/members/${id}`);

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-4">
          <Avatar className="h-14 w-14">
            <AvatarFallback>
              {member.firstName[0]}
              {member.lastName[0]}
            </AvatarFallback>
          </Avatar>
          <div>
            <h1 className="text-xl font-bold tracking-tight">
              {member.firstName} {member.lastName}
            </h1>
            <p className="text-sm text-muted-foreground">{member.email}</p>
          </div>
        </div>
        <MemberStatusSelect memberId={member.id} status={member.status} />
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Contact</CardTitle>
          </CardHeader>
          <CardContent className="space-y-1 text-sm">
            <p>Phone: {member.phone ?? "-"}</p>
            <p>Joined: {new Date(member.joinedAtUtc).toLocaleDateString()}</p>
            <p>Account linked: {member.userId ? "Yes" : "No (front-desk record)"}</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Dance styles</CardTitle>
          </CardHeader>
          <CardContent className="flex flex-wrap gap-1.5">
            {member.danceStyles.length === 0 ? (
              <p className="text-sm text-muted-foreground">No styles set.</p>
            ) : (
              member.danceStyles.map((style) => (
                <Badge key={style.danceStyleId} variant="secondary">
                  {style.danceStyleName} - {style.skillLevel}
                </Badge>
              ))
            )}
          </CardContent>
        </Card>
      </div>

      {member.notes && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Staff notes</CardTitle>
          </CardHeader>
          <CardContent className="text-sm text-muted-foreground">{member.notes}</CardContent>
        </Card>
      )}
    </div>
  );
}
