"use client";

import { useState, useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { createClassType, updateClassType } from "@/app/admin/class-types/actions";
import type { ClassType } from "@/lib/types";

export function ClassTypeDialog({ classType }: { classType?: ClassType }) {
  const [open, setOpen] = useState(false);
  const [isPending, startTransition] = useTransition();
  const [name, setName] = useState(classType?.name ?? "");
  const [description, setDescription] = useState(classType?.description ?? "");
  const [isActive, setIsActive] = useState(classType?.isActive ?? true);

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    startTransition(async () => {
      const result = classType
        ? await updateClassType(classType.id, name, description, isActive)
        : await createClassType(name, description);
      if (result.success) {
        toast.success(result.message);
        setOpen(false);
        if (!classType) {
          setName("");
          setDescription("");
        }
      } else {
        toast.error(result.message);
      }
    });
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger render={<Button variant={classType ? "outline" : "default"} size={classType ? "sm" : "default"} />}>
        {classType ? "Edit" : "Add class type"}
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{classType ? "Edit class type" : "Add a class type"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Name</Label>
            <Input id="name" required value={name} onChange={(e) => setName(e.target.value)} />
          </div>
          <div className="space-y-2">
            <Label htmlFor="description">Description (optional)</Label>
            <Input id="description" value={description} onChange={(e) => setDescription(e.target.value)} />
          </div>
          {classType && (
            <div className="flex items-center gap-2">
              <Switch id="isActive" checked={isActive} onCheckedChange={setIsActive} />
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
