"use client";

import { useState, useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { setMemberDanceStyles } from "@/app/admin/members/[id]/actions";
import type { DanceStyle, Member, SkillLevel } from "@/lib/types";

const SKILL_LEVELS: SkillLevel[] = ["Beginner", "Intermediate", "Advanced"];

export function MemberDanceStylesForm({ member, danceStyles }: { member: Member; danceStyles: DanceStyle[] }) {
  const [isPending, startTransition] = useTransition();
  const [selections, setSelections] = useState<Record<string, SkillLevel | undefined>>(
    Object.fromEntries(member.danceStyles.map((s) => [s.danceStyleId, s.skillLevel]))
  );

  function toggle(id: string) {
    setSelections((prev) => {
      const next = { ...prev };
      if (next[id]) delete next[id];
      else next[id] = "Beginner";
      return next;
    });
  }

  function setLevel(id: string, level: SkillLevel) {
    setSelections((prev) => ({ ...prev, [id]: level }));
  }

  function handleSave() {
    startTransition(async () => {
      const styles = Object.entries(selections)
        .filter((entry): entry is [string, SkillLevel] => Boolean(entry[1]))
        .map(([danceStyleId, skillLevel]) => ({ danceStyleId, skillLevel }));

      const result = await setMemberDanceStyles(member.id, styles);
      if (result.success) toast.success(result.message);
      else toast.error(result.message);
    });
  }

  return (
    <div className="space-y-3">
      <div className="space-y-2">
        {danceStyles.map((style) => (
          <div key={style.id} className="flex items-center justify-between gap-3 rounded-md border p-2.5">
            <label className="flex items-center gap-2 text-sm">
              <Checkbox checked={Boolean(selections[style.id])} onCheckedChange={() => toggle(style.id)} />
              {style.name}
            </label>
            {selections[style.id] && (
              <Select value={selections[style.id]} onValueChange={(v) => v && setLevel(style.id, v as SkillLevel)}>
                <SelectTrigger className="w-36">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {SKILL_LEVELS.map((level) => (
                    <SelectItem key={level} value={level}>
                      {level}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          </div>
        ))}
      </div>
      <Button size="sm" onClick={handleSave} disabled={isPending}>
        {isPending ? "Saving..." : "Save dance styles"}
      </Button>
    </div>
  );
}
