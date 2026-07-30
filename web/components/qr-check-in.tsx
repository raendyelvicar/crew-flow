"use client";

import { useEffect, useRef, useState, useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { RosterEntry } from "@/lib/types";

type CheckInResult = { success: boolean; message: string };

export function QrCheckIn({
  roster,
  onCheckIn,
}: {
  roster: RosterEntry[];
  onCheckIn: (bookingId: string) => Promise<CheckInResult>;
}) {
  const [scannerOpen, setScannerOpen] = useState(false);
  const [manualCode, setManualCode] = useState("");
  const [isPending, startTransition] = useTransition();
  const processedRef = useRef(false);

  function processCode(rawCode: string) {
    const bookingId = rawCode.trim();
    if (!bookingId) return;

    const entry = roster.find((r) => r.bookingId === bookingId);
    if (!entry) {
      toast.error("This code doesn't match anyone booked into this class.");
      return;
    }
    if (entry.status === "Attended") {
      toast.info(`${entry.memberName} is already checked in.`);
      return;
    }

    startTransition(async () => {
      const result = await onCheckIn(bookingId);
      if (result.success) toast.success(`Checked in ${entry.memberName}.`);
      else toast.error(result.message);
    });
  }

  // Mounts the camera scanner only while the "Scan QR" dialog is open, releasing the
  // camera as soon as it closes or a code is successfully read.
  useEffect(() => {
    if (!scannerOpen) return;

    let cancelled = false;
    processedRef.current = false;

    import("html5-qrcode").then(({ Html5QrcodeScanner }) => {
      if (cancelled) return;

      const scanner = new Html5QrcodeScanner("qr-reader", { fps: 10, qrbox: 250 }, false);
      scanner.render(
        (decodedText) => {
          if (processedRef.current) return;
          processedRef.current = true;
          processCode(decodedText);
          scanner.clear().catch(() => undefined);
          setScannerOpen(false);
        },
        () => {
          // Per-frame decode failures are expected while the camera searches for a code.
        }
      );

      return () => {
        scanner.clear().catch(() => undefined);
      };
    });

    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [scannerOpen]);

  return (
    <div className="space-y-3 rounded-lg border p-4">
      <p className="text-sm font-medium">Check in a student</p>
      <div className="flex flex-col gap-2 sm:flex-row">
        <div className="flex-1 space-y-1">
          <Label htmlFor="manualCode" className="sr-only">
            Booking code
          </Label>
          <Input
            id="manualCode"
            placeholder="Scan or type booking code"
            value={manualCode}
            onChange={(e) => setManualCode(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter" && manualCode.trim()) {
                e.preventDefault();
                processCode(manualCode);
                setManualCode("");
              }
            }}
          />
        </div>
        <div className="flex gap-2">
          <Button
            type="button"
            disabled={isPending || !manualCode.trim()}
            onClick={() => {
              processCode(manualCode);
              setManualCode("");
            }}
          >
            Check in
          </Button>
          <Dialog open={scannerOpen} onOpenChange={setScannerOpen}>
            <DialogTrigger render={<Button type="button" variant="outline" />}>Scan QR</DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Scan a member&apos;s check-in QR code</DialogTitle>
              </DialogHeader>
              <div id="qr-reader" className="w-full" />
            </DialogContent>
          </Dialog>
        </div>
      </div>
    </div>
  );
}
