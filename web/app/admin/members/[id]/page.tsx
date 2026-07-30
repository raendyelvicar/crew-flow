import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { MemberStatusSelect } from "@/components/member-status-select";
import { MemberProfileForm } from "@/components/member-profile-form";
import { MemberDanceStylesForm } from "@/components/member-dance-styles-form";
import { apiClient } from "@/lib/api-client";
import type { DanceStyle, Member } from "@/lib/types";

export default async function MemberDetailPage({ params }: PageProps<"/admin/members/[id]">) {
  const { id } = await params;
  const [member, danceStyles] = await Promise.all([
    apiClient.get<Member>(`/api/v1/members/${id}`),
    apiClient.get<DanceStyle[]>("/api/v1/dance-styles?activeOnly=true"),
  ]);

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
            <p className="text-xs text-muted-foreground">
              Joined {new Date(member.joinedAtUtc).toLocaleDateString()} -{" "}
              {member.userId ? "Account linked" : "Front-desk record (no account yet)"}
            </p>
          </div>
        </div>
        <MemberStatusSelect memberId={member.id} status={member.status} />
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Profile</CardTitle>
          </CardHeader>
          <CardContent>
            <MemberProfileForm member={member} />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Dance styles</CardTitle>
          </CardHeader>
          <CardContent>
            <MemberDanceStylesForm member={member} danceStyles={danceStyles} />
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
