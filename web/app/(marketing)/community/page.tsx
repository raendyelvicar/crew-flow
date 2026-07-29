import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";

export const dynamic = "force-dynamic";
import { publicGet } from "@/lib/api-client";
import type { MemberDirectoryEntry } from "@/lib/types";

export default async function CommunityPage() {
  const members = (await publicGet<MemberDirectoryEntry[]>("/api/v1/members/directory", 30)) ?? [];

  return (
    <section className="px-4 py-12 sm:py-16">
      <div className="mx-auto max-w-5xl">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-bold tracking-tight sm:text-4xl">The Crew</h1>
          <p className="mt-2 text-sm text-muted-foreground sm:text-base">
            Dancers in our community who&apos;ve chosen to share their profile.
          </p>
        </div>

        {members.length === 0 ? (
          <p className="text-center text-sm text-muted-foreground">No public profiles yet - be the first to opt in from your dashboard.</p>
        ) : (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {members.map((member) => (
              <Card key={member.id}>
                <CardContent className="flex flex-col items-center gap-3 pt-6 text-center">
                  <Avatar className="h-16 w-16">
                    <AvatarFallback>
                      {member.firstName[0]}
                      {member.lastName[0]}
                    </AvatarFallback>
                  </Avatar>
                  <div>
                    <p className="font-medium">
                      {member.firstName} {member.lastName}
                    </p>
                    {member.bio && <p className="mt-1 text-sm text-muted-foreground">{member.bio}</p>}
                  </div>
                  {member.danceStyles.length > 0 && (
                    <div className="flex flex-wrap justify-center gap-1.5">
                      {member.danceStyles.map((style) => (
                        <Badge key={style.danceStyleId} variant="secondary">
                          {style.danceStyleName}
                        </Badge>
                      ))}
                    </div>
                  )}
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>
    </section>
  );
}
