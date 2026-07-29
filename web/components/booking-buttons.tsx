"use client";

import { useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { bookClass, cancelBooking } from "@/app/(portal)/classes/actions";

export function BookButton({ classOccurrenceId, disabled }: { classOccurrenceId: string; disabled?: boolean }) {
  const [isPending, startTransition] = useTransition();

  return (
    <Button
      size="sm"
      disabled={disabled || isPending}
      onClick={() =>
        startTransition(async () => {
          const result = await bookClass(classOccurrenceId);
          if (result.success) toast.success(result.message);
          else toast.error(result.message);
        })
      }
    >
      {isPending ? "Booking..." : "Book"}
    </Button>
  );
}

export function CancelBookingButton({ bookingId }: { bookingId: string }) {
  const [isPending, startTransition] = useTransition();

  return (
    <Button
      size="sm"
      variant="outline"
      disabled={isPending}
      onClick={() =>
        startTransition(async () => {
          const result = await cancelBooking(bookingId);
          if (result.success) toast.success(result.message);
          else toast.error(result.message);
        })
      }
    >
      {isPending ? "Cancelling..." : "Cancel"}
    </Button>
  );
}
