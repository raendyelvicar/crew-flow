"use client";

import { useState, useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { updateMemberProfile, type MemberProfileFormInput } from "@/app/admin/members/[id]/actions";
import type { Member } from "@/lib/types";

export function MemberProfileForm({ member }: { member: Member }) {
  const [isPending, startTransition] = useTransition();
  const [form, setForm] = useState<MemberProfileFormInput>({
    phone: member.phone ?? "",
    dateOfBirth: member.dateOfBirth ?? "",
    emergencyContactName: "",
    emergencyContactPhone: "",
    bio: member.bio ?? "",
    avatarUrl: member.avatarUrl ?? "",
    instagramHandle: member.instagramHandle ?? "",
    tikTokHandle: member.tikTokHandle ?? "",
    websiteUrl: member.websiteUrl ?? "",
    isProfilePublic: member.isProfilePublic,
  });

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    startTransition(async () => {
      const result = await updateMemberProfile(member.id, form);
      if (result.success) toast.success(result.message);
      else toast.error(result.message);
    });
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-2">
          <Label htmlFor="phone">Phone</Label>
          <Input id="phone" value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
        </div>
        <div className="space-y-2">
          <Label htmlFor="dob">Date of birth</Label>
          <Input
            id="dob"
            type="date"
            value={form.dateOfBirth}
            onChange={(e) => setForm({ ...form, dateOfBirth: e.target.value })}
          />
        </div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-2">
          <Label htmlFor="ecName">Emergency contact name</Label>
          <Input
            id="ecName"
            value={form.emergencyContactName}
            onChange={(e) => setForm({ ...form, emergencyContactName: e.target.value })}
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="ecPhone">Emergency contact phone</Label>
          <Input
            id="ecPhone"
            value={form.emergencyContactPhone}
            onChange={(e) => setForm({ ...form, emergencyContactPhone: e.target.value })}
          />
        </div>
      </div>
      <div className="space-y-2">
        <Label htmlFor="bio">Bio</Label>
        <Textarea id="bio" rows={3} value={form.bio} onChange={(e) => setForm({ ...form, bio: e.target.value })} />
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-2">
          <Label htmlFor="instagram">Instagram handle</Label>
          <Input
            id="instagram"
            value={form.instagramHandle}
            onChange={(e) => setForm({ ...form, instagramHandle: e.target.value })}
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="tiktok">TikTok handle</Label>
          <Input
            id="tiktok"
            value={form.tikTokHandle}
            onChange={(e) => setForm({ ...form, tikTokHandle: e.target.value })}
          />
        </div>
      </div>
      <div className="flex items-center gap-2">
        <Switch
          id="isProfilePublic"
          checked={form.isProfilePublic}
          onCheckedChange={(v) => setForm({ ...form, isProfilePublic: v })}
        />
        <Label htmlFor="isProfilePublic">Visible in public community directory</Label>
      </div>
      <Button type="submit" disabled={isPending}>
        {isPending ? "Saving..." : "Save profile"}
      </Button>
    </form>
  );
}
