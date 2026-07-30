"use client";

import { useState } from "react";
import { QRCodeSVG } from "qrcode.react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";

export function BookingQrDialog({ bookingId, activityName }: { bookingId: string; activityName: string }) {
  const [open, setOpen] = useState(false);

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger render={<Button size="sm" variant="outline" />}>Check-in QR</DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{activityName}</DialogTitle>
          <DialogDescription>Show this to your instructor to check in when you arrive.</DialogDescription>
        </DialogHeader>
        <div className="flex justify-center py-4">
          <div className="rounded-lg bg-white p-4">
            <QRCodeSVG value={bookingId} size={220} />
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
