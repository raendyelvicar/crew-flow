"use client";

import { useState, useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { createClassSchedule } from "@/app/admin/schedule/actions";
import type { Activity } from "@/lib/types";

const DAYS = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

export function CreateClassScheduleDialog({ activities }: { activities: Activity[] }) {
  const [open, setOpen] = useState(false);
  const [isPending, startTransition] = useTransition();
  const [form, setForm] = useState({
    activityId: activities[0]?.id ?? "",
    instructorUserId: "",
    dayOfWeek: "1",
    startTimeLocal: "18:00",
    durationMinutes: "60",
    capacity: "12",
    timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
  });

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    startTransition(async () => {
      const result = await createClassSchedule({
        activityId: form.activityId,
        instructorUserId: form.instructorUserId,
        dayOfWeek: Number(form.dayOfWeek),
        startTimeLocal: `${form.startTimeLocal}:00`,
        durationMinutes: Number(form.durationMinutes),
        capacity: Number(form.capacity),
        timezone: form.timezone,
      });
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
      <DialogTrigger render={<Button disabled={activities.length === 0} />}>Add weekly class</DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Add a recurring weekly class</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label>Activity</Label>
            <Select value={form.activityId} onValueChange={(v) => setForm({ ...form, activityId: v ?? "" })}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {activities.map((activity) => (
                  <SelectItem key={activity.id} value={activity.id}>
                    {activity.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label htmlFor="instructorUserId">Instructor user ID</Label>
            <Input
              id="instructorUserId"
              placeholder="Staff account's user ID"
              required
              value={form.instructorUserId}
              onChange={(e) => setForm({ ...form, instructorUserId: e.target.value })}
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-2">
              <Label>Day of week</Label>
              <Select value={form.dayOfWeek} onValueChange={(v) => setForm({ ...form, dayOfWeek: v ?? "1" })}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {DAYS.map((day, i) => (
                    <SelectItem key={day} value={String(i)}>
                      {day}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="startTime">Start time</Label>
              <Input
                id="startTime"
                type="time"
                required
                value={form.startTimeLocal}
                onChange={(e) => setForm({ ...form, startTimeLocal: e.target.value })}
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-2">
              <Label htmlFor="duration">Duration (min)</Label>
              <Input
                id="duration"
                type="number"
                min={15}
                required
                value={form.durationMinutes}
                onChange={(e) => setForm({ ...form, durationMinutes: e.target.value })}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="capacity">Capacity</Label>
              <Input
                id="capacity"
                type="number"
                min={1}
                required
                value={form.capacity}
                onChange={(e) => setForm({ ...form, capacity: e.target.value })}
              />
            </div>
          </div>
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
