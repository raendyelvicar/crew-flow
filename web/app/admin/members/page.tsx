import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { CreateMemberDialog } from "@/components/create-member-dialog";
import { apiClient } from "@/lib/api-client";
import type { Member } from "@/lib/types";

export default async function AdminMembersPage() {
  const members = await apiClient.get<Member[]>("/api/v1/members");

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Members</h1>
          <p className="text-sm text-muted-foreground">{members.length} total</p>
        </div>
        <CreateMemberDialog />
      </div>

      {/* Mobile: stacked cards */}
      <div className="space-y-3 sm:hidden">
        {members.map((member) => (
          <Link
            key={member.id}
            href={`/admin/members/${member.id}`}
            className="flex items-center justify-between rounded-lg border p-4"
          >
            <div>
              <p className="font-medium">
                {member.firstName} {member.lastName}
              </p>
              <p className="text-sm text-muted-foreground">{member.email}</p>
            </div>
            <Badge variant={member.status === "Active" ? "default" : "secondary"}>{member.status}</Badge>
          </Link>
        ))}
      </div>

      {/* Desktop: table */}
      <div className="hidden overflow-x-auto rounded-lg border sm:block">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Phone</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Joined</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {members.map((member) => (
              <TableRow key={member.id} className="cursor-pointer">
                <TableCell>
                  <Link href={`/admin/members/${member.id}`} className="font-medium hover:underline">
                    {member.firstName} {member.lastName}
                  </Link>
                </TableCell>
                <TableCell>{member.email}</TableCell>
                <TableCell>{member.phone ?? "-"}</TableCell>
                <TableCell>
                  <Badge variant={member.status === "Active" ? "default" : "secondary"}>{member.status}</Badge>
                </TableCell>
                <TableCell>{new Date(member.joinedAtUtc).toLocaleDateString()}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
