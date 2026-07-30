"use client";

import { useState, useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { createCoach, updateCoach, type CoachFormInput } from "@/app/admin/coaches/actions";
import type { DanceStyle, Instructor } from "@/lib/types";

export function CoachDialog({ coach, danceStyles }: { coach?: Instructor; danceStyles: DanceStyle[] }) {
  const [open, setOpen] = useState(false);
  const [isPending, startTransition] = useTransition();

  const [firstName, setFirstName] = useState(coach?.firstName ?? "");
  const [lastName, setLastName] = useState(coach?.lastName ?? "");
  const [email, setEmail] = useState("");
  const [isActive, setIsActive] = useState(coach?.isActive ?? true);
  const [form, setForm] = useState<CoachFormInput>({
    bio: coach?.bio ?? "",
    avatarUrl: coach?.avatarUrl ?? "",
    yearsExperience: coach?.yearsExperience?.toString() ?? "",
    instagramHandle: coach?.instagramHandle ?? "",
    websiteUrl: coach?.websiteUrl ?? "",
    danceStyleIds: coach?.danceStyles.map((s) => s.danceStyleId) ?? [],
  });

  function toggleStyle(id: string) {
    setForm((f) => ({
      ...f,
      danceStyleIds: f.danceStyleIds.includes(id) ? f.danceStyleIds.filter((s) => s !== id) : [...f.danceStyleIds, id],
    }));
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    startTransition(async () => {
      const result = coach ? await updateCoach(coach.userId, isActive, form) : await createCoach(firstName, lastName, email, form);
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
      <DialogTrigger render={<Button variant={coach ? "outline" : "default"} size={coach ? "sm" : "default"} />}>
        {coach ? "Edit" : "Add coach"}
      </DialogTrigger>
      <DialogContent className="max-h-[85vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{coach ? "Edit coach" : "Add a coach"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          {!coach && (
            <>
              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-2">
                  <Label htmlFor="firstName">First name</Label>
                  <Input id="firstName" required value={firstName} onChange={(e) => setFirstName(e.target.value)} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="lastName">Last name</Label>
                  <Input id="lastName" required value={lastName} onChange={(e) => setLastName(e.target.value)} />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="email">Email</Label>
                <Input id="email" type="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
              </div>
            </>
          )}
          <div className="space-y-2">
            <Label htmlFor="bio">Bio</Label>
            <Textarea id="bio" value={form.bio} onChange={(e) => setForm({ ...form, bio: e.target.value })} rows={3} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-2">
              <Label htmlFor="years">Years experience</Label>
              <Input
                id="years"
                type="number"
                min={0}
                value={form.yearsExperience}
                onChange={(e) => setForm({ ...form, yearsExperience: e.target.value })}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="instagram">Instagram handle</Label>
              <Input
                id="instagram"
                value={form.instagramHandle}
                onChange={(e) => setForm({ ...form, instagramHandle: e.target.value })}
              />
            </div>
          </div>
          <div className="space-y-2">
            <Label>Styles taught</Label>
            <div className="grid grid-cols-2 gap-2 rounded-md border p-3 sm:grid-cols-3">
              {danceStyles.map((style) => (
                <label key={style.id} className="flex items-center gap-2 text-sm">
                  <Checkbox checked={form.danceStyleIds.includes(style.id)} onCheckedChange={() => toggleStyle(style.id)} />
                  {style.name}
                </label>
              ))}
            </div>
          </div>
          {coach && (
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
