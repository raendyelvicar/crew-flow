"use client";

import { useTransition } from "react";
import { toast } from "sonner";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { markAttendance } from "@/app/admin/schedule/occurrences/[id]/actions";
import type { RosterEntry } from "@/lib/types";

export function RosterTable({ occurrenceId, roster }: { occurrenceId: string; roster: RosterEntry[] }) {
  const [isPending, startTransition] = useTransition();

  function handleMark(bookingId: string, status: "Attended" | "NoShow") {
    startTransition(async () => {
      const result = await markAttendance(bookingId, occurrenceId, status);
      if (result.success) toast.success(result.message);
      else toast.error(result.message);
    });
  }

  return (
    <div className="overflow-x-auto rounded-lg border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Member</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="text-right">Attendance</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {roster.map((entry) => (
            <TableRow key={entry.bookingId}>
              <TableCell className="font-medium">{entry.memberName}</TableCell>
              <TableCell>
                <Badge variant={entry.status === "Booked" ? "default" : "secondary"}>
                  {entry.status}
                  {entry.status === "Waitlisted" && entry.waitlistPosition ? ` #${entry.waitlistPosition}` : ""}
                </Badge>
              </TableCell>
              <TableCell className="text-right">
                {entry.status === "Booked" && (
                  <div className="flex justify-end gap-2">
                    <Button size="sm" variant="outline" disabled={isPending} onClick={() => handleMark(entry.bookingId, "Attended")}>
                      Attended
                    </Button>
                    <Button size="sm" variant="ghost" disabled={isPending} onClick={() => handleMark(entry.bookingId, "NoShow")}>
                      No-show
                    </Button>
                  </div>
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
