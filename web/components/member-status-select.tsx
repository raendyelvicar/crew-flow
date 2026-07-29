"use client";

import { useTransition } from "react";
import { toast } from "sonner";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { updateMemberStatus } from "@/app/admin/members/[id]/actions";
import type { MemberStatus } from "@/lib/types";

export function MemberStatusSelect({ memberId, status }: { memberId: string; status: MemberStatus }) {
  const [isPending, startTransition] = useTransition();

  return (
    <Select
      defaultValue={status}
      disabled={isPending}
      onValueChange={(value) =>
        startTransition(async () => {
          const result = await updateMemberStatus(memberId, value as MemberStatus);
          if (result.success) toast.success(result.message);
          else toast.error(result.message);
        })
      }
    >
      <SelectTrigger className="w-40">
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value="Active">Active</SelectItem>
        <SelectItem value="Inactive">Inactive</SelectItem>
        <SelectItem value="Archived">Archived</SelectItem>
      </SelectContent>
    </Select>
  );
}
