"use client";

import { useState, useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { saveActivity, type ActivityFormInput } from "@/app/admin/schedule/actions";
import type { Activity, ClassType, DanceStyle } from "@/lib/types";

export function ActivityDialog({
  activity,
  danceStyles,
  classTypes,
}: {
  activity?: Activity;
  danceStyles: DanceStyle[];
  classTypes: ClassType[];
}) {
  const [open, setOpen] = useState(false);
  const [isPending, startTransition] = useTransition();
  const [form, setForm] = useState<ActivityFormInput>({
    name: activity?.name ?? "",
    description: activity?.description ?? "",
    classGenreId: activity?.classGenreId ?? danceStyles[0]?.id ?? "",
    classTypeId: activity?.classTypeId ?? classTypes[0]?.id ?? "",
    defaultCapacity: activity?.defaultCapacity.toString() ?? "12",
    defaultDurationMinutes: activity?.defaultDurationMinutes.toString() ?? "60",
    isActive: activity?.isActive ?? true,
  });

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    startTransition(async () => {
      const result = await saveActivity(activity?.id ?? null, form);
      if (result.success) {
        toast.success(result.message);
        setOpen(false);
      } else {
        toast.error(result.message);
      }
    });
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger render={<Button variant={activity ? "outline" : "outline"} size={activity ? "sm" : "default"} />}>
        {activity ? "Edit" : "Add activity"}
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{activity ? "Edit activity" : "Add an activity"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Name</Label>
            <Input id="name" required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-2">
              <Label>Class genre</Label>
              <Select value={form.classGenreId} onValueChange={(v) => v && setForm({ ...form, classGenreId: v })}>
                <SelectTrigger>
                  <SelectValue>{(id: string) => danceStyles.find((s) => s.id === id)?.name}</SelectValue>
                </SelectTrigger>
                <SelectContent>
                  {danceStyles.map((style) => (
                    <SelectItem key={style.id} value={style.id}>
                      {style.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Class type</Label>
              <Select value={form.classTypeId} onValueChange={(v) => v && setForm({ ...form, classTypeId: v })}>
                <SelectTrigger>
                  <SelectValue>{(id: string) => classTypes.find((t) => t.id === id)?.name}</SelectValue>
                </SelectTrigger>
                <SelectContent>
                  {classTypes.map((type) => (
                    <SelectItem key={type.id} value={type.id}>
                      {type.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-2">
              <Label htmlFor="capacity">Default capacity</Label>
              <Input
                id="capacity"
                type="number"
                min={1}
                required
                value={form.defaultCapacity}
                onChange={(e) => setForm({ ...form, defaultCapacity: e.target.value })}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="duration">Duration (min)</Label>
              <Input
                id="duration"
                type="number"
                min={15}
                required
                value={form.defaultDurationMinutes}
                onChange={(e) => setForm({ ...form, defaultDurationMinutes: e.target.value })}
              />
            </div>
          </div>
          {activity && (
            <div className="flex items-center gap-2">
              <Switch id="isActive" checked={form.isActive} onCheckedChange={(v) => setForm({ ...form, isActive: v })} />
              <Label htmlFor="isActive">Active</Label>
            </div>
          )}
          <DialogFooter>
            <Button type="submit" disabled={isPending}>
              {isPending ? "Saving..." : "Save"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
