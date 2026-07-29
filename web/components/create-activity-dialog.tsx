"use client";

import { useState, useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { createActivity } from "@/app/admin/schedule/actions";

export function CreateActivityDialog() {
  const [open, setOpen] = useState(false);
  const [isPending, startTransition] = useTransition();
  const [form, setForm] = useState({ name: "", category: "", defaultCapacity: "12", defaultDurationMinutes: "60" });

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    startTransition(async () => {
      const result = await createActivity({
        name: form.name,
        category: form.category,
        defaultCapacity: Number(form.defaultCapacity),
        defaultDurationMinutes: Number(form.defaultDurationMinutes),
      });
      if (result.success) {
        toast.success(result.message);
        setOpen(false);
        setForm({ name: "", category: "", defaultCapacity: "12", defaultDurationMinutes: "60" });
      } else {
        toast.error(result.message);
      }
    });
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger render={<Button variant="outline" />}>Add activity</DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Add an activity</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Name</Label>
            <Input id="name" required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
          </div>
          <div className="space-y-2">
            <Label htmlFor="category">Category</Label>
            <Input
              id="category"
              placeholder="Salsa, Hip-Hop, Ballet..."
              required
              value={form.category}
              onChange={(e) => setForm({ ...form, category: e.target.value })}
            />
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
