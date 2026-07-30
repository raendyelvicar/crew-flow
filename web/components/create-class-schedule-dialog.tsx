"use client";

import { useState, useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { createClassSchedule, updateClassSchedule, type ClassScheduleFormInput } from "@/app/admin/schedule/actions";
import type { Activity, ClassSchedule, Instructor } from "@/lib/types";

const DAYS = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

export function CreateClassScheduleDialog({
  activities,
  instructors,
  schedule,
}: {
  activities: Activity[];
  instructors: Instructor[];
  schedule?: ClassSchedule;
}) {
  const [open, setOpen] = useState(false);
  const [isPending, startTransition] = useTransition();
  const [form, setForm] = useState<ClassScheduleFormInput>({
    activityId: schedule?.activityId ?? activities[0]?.id ?? "",
    instructorUserId: schedule?.instructorUserId ?? instructors[0]?.userId ?? "",
    dayOfWeek: schedule ? DAYS.indexOf(schedule.dayOfWeek).toString() : "1",
    startTimeLocal: schedule?.startTimeLocal.slice(0, 5) ?? "18:00",
    durationMinutes: schedule?.durationMinutes.toString() ?? "60",
    capacity: schedule?.capacity.toString() ?? "12",
    timezone: schedule?.timezone ?? Intl.DateTimeFormat().resolvedOptions().timeZone,
    isActive: schedule?.isActive ?? true,
  });

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const payload = { ...form, startTimeLocal: `${form.startTimeLocal}:00` };

    startTransition(async () => {
      const result = schedule
        ? await updateClassSchedule(schedule.id, payload, schedule.effectiveFromDate)
        : await createClassSchedule(payload);
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
      <DialogTrigger
        render={
          <Button
            disabled={!schedule && (activities.length === 0 || instructors.length === 0)}
            variant={schedule ? "outline" : "default"}
            size={schedule ? "sm" : "default"}
          />
        }
      >
        {schedule ? "Edit" : "Add weekly class"}
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{schedule ? "Edit recurring class" : "Add a recurring weekly class"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          {!schedule && (
            <div className="space-y-2">
              <Label>Activity</Label>
              <Select value={form.activityId} onValueChange={(v) => v && setForm({ ...form, activityId: v })}>
                <SelectTrigger>
                  <SelectValue>{(id: string) => activities.find((a) => a.id === id)?.name}</SelectValue>
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
          )}
          <div className="space-y-2">
            <Label>Instructor</Label>
            <Select
              value={form.instructorUserId}
              onValueChange={(v) => v && setForm({ ...form, instructorUserId: v })}
            >
              <SelectTrigger>
                <SelectValue>
                  {(id: string) => {
                    const instructor = instructors.find((i) => i.userId === id);
                    return instructor ? `${instructor.firstName} ${instructor.lastName}` : undefined;
                  }}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {instructors.map((instructor) => (
                  <SelectItem key={instructor.userId} value={instructor.userId}>
                    {instructor.firstName} {instructor.lastName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-2">
              <Label>Day of week</Label>
              <Select value={form.dayOfWeek} onValueChange={(v) => v && setForm({ ...form, dayOfWeek: v })}>
                <SelectTrigger>
                  <SelectValue>{(index: string) => DAYS[Number(index)]}</SelectValue>
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
          {schedule && (
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
