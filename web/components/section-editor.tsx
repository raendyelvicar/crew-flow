"use client";

import { useState, useTransition } from "react";
import { toast } from "sonner";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { deleteSection, upsertSection } from "@/app/admin/cms/actions";
import type { PageSection, SectionType } from "@/lib/types";

const SECTION_TYPES: SectionType[] = ["Hero", "RichText", "CtaBanner", "Testimonials", "ImageGallery", "PricingTable"];

const SAMPLE_CONTENT: Record<SectionType, string> = {
  Hero: JSON.stringify({ heading: "Welcome to Crew Flow", subheading: "Dance studio and community", ctaLabel: "Join now", ctaHref: "/register" }, null, 2),
  RichText: JSON.stringify({ text: "Write a paragraph here.\n\nAnd another one." }, null, 2),
  CtaBanner: JSON.stringify({ heading: "Ready to join?", subheading: "First class is on us.", ctaLabel: "Sign up", ctaHref: "/register" }, null, 2),
  Testimonials: JSON.stringify({ items: [{ quote: "Best studio in town!", author: "Alex" }] }, null, 2),
  ImageGallery: JSON.stringify({ images: [{ url: "https://example.com/photo.jpg", alt: "Studio" }] }, null, 2),
  PricingTable: JSON.stringify({}, null, 2),
};

function SectionForm({
  pageId,
  section,
  onSaved,
}: {
  pageId: string;
  section?: PageSection;
  onSaved?: () => void;
}) {
  const [isPending, startTransition] = useTransition();
  const [sectionType, setSectionType] = useState<SectionType>(section?.sectionType ?? "Hero");
  const [sortOrder, setSortOrder] = useState(section?.sortOrder ?? 0);
  const [contentJson, setContentJson] = useState(section?.contentJson ?? SAMPLE_CONTENT.Hero);

  function handleSave() {
    startTransition(async () => {
      const result = await upsertSection(pageId, section?.id ?? null, sortOrder, sectionType, contentJson);
      if (result.success) {
        toast.success(result.message);
        onSaved?.();
      } else {
        toast.error(result.message);
      }
    });
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-3">
        <Select
          value={sectionType}
          onValueChange={(v) => {
            if (!v) return;
            const type = v as SectionType;
            setSectionType(type);
            if (!section) setContentJson(SAMPLE_CONTENT[type]);
          }}
        >
          <SelectTrigger className="w-48">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {SECTION_TYPES.map((type) => (
              <SelectItem key={type} value={type}>
                {type}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <input
          type="number"
          value={sortOrder}
          onChange={(e) => setSortOrder(Number(e.target.value))}
          className="w-20 rounded-md border px-2 py-1 text-sm"
          aria-label="Sort order"
        />
      </div>
      <Textarea
        value={contentJson}
        onChange={(e) => setContentJson(e.target.value)}
        rows={8}
        className="font-mono text-xs"
      />
      <div className="flex gap-2">
        <Button size="sm" onClick={handleSave} disabled={isPending}>
          {isPending ? "Saving..." : "Save section"}
        </Button>
        {section && (
          <Button
            size="sm"
            variant="ghost"
            disabled={isPending}
            onClick={() =>
              startTransition(async () => {
                const result = await deleteSection(pageId, section.id);
                if (result.success) toast.success(result.message);
                else toast.error(result.message);
              })
            }
          >
            Delete
          </Button>
        )}
      </div>
    </div>
  );
}

export function SectionEditor({ pageId, sections }: { pageId: string; sections: PageSection[] }) {
  const [addingNew, setAddingNew] = useState(false);

  return (
    <div className="space-y-4">
      {sections
        .sort((a, b) => a.sortOrder - b.sortOrder)
        .map((section) => (
          <Card key={section.id}>
            <CardHeader className="flex-row items-center justify-between space-y-0">
              <CardTitle className="text-sm font-medium">
                <Badge variant="secondary">{section.sectionType}</Badge>{" "}
                <span className="text-muted-foreground">order {section.sortOrder}</span>
              </CardTitle>
            </CardHeader>
            <CardContent>
              <SectionForm pageId={pageId} section={section} />
            </CardContent>
          </Card>
        ))}

      {addingNew ? (
        <Card>
          <CardHeader>
            <CardTitle className="text-sm font-medium">New section</CardTitle>
          </CardHeader>
          <CardContent>
            <SectionForm pageId={pageId} onSaved={() => setAddingNew(false)} />
          </CardContent>
        </Card>
      ) : (
        <Button variant="outline" onClick={() => setAddingNew(true)}>
          Add section
        </Button>
      )}
    </div>
  );
}
