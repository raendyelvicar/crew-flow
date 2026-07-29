"use client";

import { useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { setPagePublished } from "@/app/admin/cms/actions";

export function PublishToggle({ pageId, isPublished }: { pageId: string; isPublished: boolean }) {
  const [isPending, startTransition] = useTransition();

  return (
    <Button
      size="sm"
      variant={isPublished ? "outline" : "default"}
      disabled={isPending}
      onClick={() =>
        startTransition(async () => {
          const result = await setPagePublished(pageId, !isPublished);
          if (result.success) toast.success(result.message);
          else toast.error(result.message);
        })
      }
    >
      {isPending ? "Saving..." : isPublished ? "Unpublish" : "Publish"}
    </Button>
  );
}
