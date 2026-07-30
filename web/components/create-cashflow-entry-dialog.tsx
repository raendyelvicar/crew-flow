"use client";

import { useState, useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { createCashflowEntry, type CreateCashflowEntryInput } from "@/app/admin/cashflow/actions";

const SOURCE_LABELS: Record<CreateCashflowEntryInput["source"], string> = {
  ManualCash: "Cash",
  ManualCard: "Card",
  Other: "Other",
};

const CATEGORY_LABELS: Record<CreateCashflowEntryInput["category"], string> = {
  Membership: "Membership",
  CreditPack: "Credit pack",
  DropIn: "Drop-in",
  Merchandise: "Merchandise",
  Other: "Other",
};

export function CreateCashflowEntryDialog() {
  const [open, setOpen] = useState(false);
  const [isPending, startTransition] = useTransition();
  const [amount, setAmount] = useState("");
  const [source, setSource] = useState<CreateCashflowEntryInput["source"]>("ManualCash");
  const [category, setCategory] = useState<CreateCashflowEntryInput["category"]>("DropIn");
  const [description, setDescription] = useState("");

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    startTransition(async () => {
      const result = await createCashflowEntry({
        amount: Number(amount),
        source,
        category,
        description: description || undefined,
      });
      if (result.success) {
        toast.success(result.message);
        setOpen(false);
        setAmount("");
        setDescription("");
      } else {
        toast.error(result.message);
      }
    });
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger render={<Button />}>Record entry</DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Record a manual cashflow entry</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="amount">Amount (USD)</Label>
            <Input
              id="amount"
              type="number"
              step="0.01"
              required
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-2">
              <Label>Source</Label>
              <Select
                value={source}
                onValueChange={(v) => v && setSource(v as CreateCashflowEntryInput["source"])}
              >
                <SelectTrigger>
                  <SelectValue>{(value: string) => SOURCE_LABELS[value as CreateCashflowEntryInput["source"]]}</SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="ManualCash">Cash</SelectItem>
                  <SelectItem value="ManualCard">Card</SelectItem>
                  <SelectItem value="Other">Other</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Category</Label>
              <Select
                value={category}
                onValueChange={(v) => v && setCategory(v as CreateCashflowEntryInput["category"])}
              >
                <SelectTrigger>
                  <SelectValue>{(value: string) => CATEGORY_LABELS[value as CreateCashflowEntryInput["category"]]}</SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Membership">Membership</SelectItem>
                  <SelectItem value="CreditPack">Credit pack</SelectItem>
                  <SelectItem value="DropIn">Drop-in</SelectItem>
                  <SelectItem value="Merchandise">Merchandise</SelectItem>
                  <SelectItem value="Other">Other</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
          <div className="space-y-2">
            <Label htmlFor="description">Description (optional)</Label>
            <Input id="description" value={description} onChange={(e) => setDescription(e.target.value)} />
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
