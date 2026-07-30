"use client";

import { useState, useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { saveCreditPack, type CreditPackFormInput } from "@/app/admin/packages/actions";
import type { CreditPack } from "@/lib/types";

export function CreditPackDialog({ pack }: { pack?: CreditPack }) {
  const [open, setOpen] = useState(false);
  const [isPending, startTransition] = useTransition();
  const [form, setForm] = useState<CreditPackFormInput>({
    name: pack?.name ?? "",
    description: pack?.description ?? "",
    creditCount: pack?.creditCount.toString() ?? "",
    priceAmount: pack?.priceAmount.toString() ?? "",
    expiryDays: pack?.expiryDays?.toString() ?? "",
    isActive: pack?.isActive ?? true,
  });

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    startTransition(async () => {
      const result = await saveCreditPack(pack?.id ?? null, form);
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
      <DialogTrigger render={<Button variant={pack ? "outline" : "default"} size={pack ? "sm" : "default"} />}>
        {pack ? "Edit" : "Add credit pack"}
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{pack ? "Edit credit pack" : "Add a credit pack"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Name</Label>
            <Input id="name" required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
          </div>
          <div className="space-y-2">
            <Label htmlFor="description">Description</Label>
            <Input
              id="description"
              value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
            />
          </div>
          <div className="grid grid-cols-3 gap-3">
            <div className="space-y-2">
              <Label htmlFor="creditCount">Credits</Label>
              <Input
                id="creditCount"
                type="number"
                min={1}
                required
                value={form.creditCount}
                onChange={(e) => setForm({ ...form, creditCount: e.target.value })}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="price">Price (IDR)</Label>
              <Input
                id="price"
                type="number"
                min={0}
                required
                value={form.priceAmount}
                onChange={(e) => setForm({ ...form, priceAmount: e.target.value })}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="expiryDays">Expires (days)</Label>
              <Input
                id="expiryDays"
                type="number"
                min={0}
                placeholder="Never"
                value={form.expiryDays}
                onChange={(e) => setForm({ ...form, expiryDays: e.target.value })}
              />
            </div>
          </div>
          <div className="flex items-center gap-2">
            <Switch id="isActive" checked={form.isActive} onCheckedChange={(v) => setForm({ ...form, isActive: v })} />
            <Label htmlFor="isActive">Active</Label>
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
