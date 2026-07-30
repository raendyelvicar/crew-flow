"use client";

import { useState, useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { createClassGenre, updateClassGenre } from "@/app/admin/class-genres/actions";
import type { DanceStyle } from "@/lib/types";

export function ClassGenreDialog({ genre }: { genre?: DanceStyle }) {
  const [open, setOpen] = useState(false);
  const [isPending, startTransition] = useTransition();
  const [name, setName] = useState(genre?.name ?? "");
  const [isActive, setIsActive] = useState(genre?.isActive ?? true);

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    startTransition(async () => {
      const result = genre ? await updateClassGenre(genre.id, name, isActive) : await createClassGenre(name);
      if (result.success) {
        toast.success(result.message);
        setOpen(false);
        if (!genre) setName("");
      } else {
        toast.error(result.message);
      }
    });
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger render={<Button variant={genre ? "outline" : "default"} size={genre ? "sm" : "default"} />}>
        {genre ? "Edit" : "Add class genre"}
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{genre ? "Edit class genre" : "Add a class genre"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Name</Label>
            <Input id="name" required value={name} onChange={(e) => setName(e.target.value)} />
          </div>
          {genre && (
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
