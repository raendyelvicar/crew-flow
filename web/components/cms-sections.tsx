import Image from "next/image";
import Link from "next/link";
import { buttonVariants } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import type { MembershipPlan, PageSection } from "@/lib/types";
import { PricingCards } from "@/components/pricing-cards";

type HeroContent = { heading?: string; subheading?: string; imageUrl?: string; ctaLabel?: string; ctaHref?: string };
type RichTextContent = { text?: string };
type CtaBannerContent = { heading?: string; subheading?: string; ctaLabel?: string; ctaHref?: string };
type TestimonialItem = { quote?: string; author?: string };
type TestimonialsContent = { items?: TestimonialItem[] };
type ImageGalleryContent = { images?: { url?: string; alt?: string }[] };

function parse<T>(json: string): T {
  try {
    return JSON.parse(json) as T;
  } catch {
    return {} as T;
  }
}

export function CmsSections({ sections, plans }: { sections: PageSection[]; plans?: MembershipPlan[] }) {
  return (
    <div className="flex flex-col">
      {sections
        .filter((section) => section.isVisible)
        .sort((a, b) => a.sortOrder - b.sortOrder)
        .map((section) => (
          <CmsSection key={section.id} section={section} plans={plans} />
        ))}
    </div>
  );
}

function CmsSection({ section, plans }: { section: PageSection; plans?: MembershipPlan[] }) {
  switch (section.sectionType) {
    case "Hero": {
      const content = parse<HeroContent>(section.contentJson);
      return (
        <section className="px-4 py-16 sm:py-24">
          <div className="mx-auto flex max-w-4xl flex-col items-center gap-6 text-center">
            {content.heading && (
              <h1 className="text-3xl font-bold tracking-tight sm:text-5xl">{content.heading}</h1>
            )}
            {content.subheading && (
              <p className="max-w-2xl text-base text-muted-foreground sm:text-lg">{content.subheading}</p>
            )}
            {content.imageUrl && (
              <div className="relative mt-4 aspect-video w-full max-w-2xl overflow-hidden rounded-xl">
                <Image src={content.imageUrl} alt={content.heading ?? ""} fill className="object-cover" />
              </div>
            )}
            {content.ctaLabel && content.ctaHref && (
              <Link href={content.ctaHref} className={buttonVariants({ size: "lg", className: "mt-2" })}>
                {content.ctaLabel}
              </Link>
            )}
          </div>
        </section>
      );
    }

    case "RichText": {
      const content = parse<RichTextContent>(section.contentJson);
      return (
        <section className="px-4 py-12">
          <div className="mx-auto max-w-3xl space-y-4 text-sm leading-relaxed text-muted-foreground sm:text-base">
            {(content.text ?? "").split("\n\n").map((paragraph, i) => (
              <p key={i}>{paragraph}</p>
            ))}
          </div>
        </section>
      );
    }

    case "CtaBanner": {
      const content = parse<CtaBannerContent>(section.contentJson);
      return (
        <section className="bg-muted px-4 py-12 sm:py-16">
          <div className="mx-auto flex max-w-3xl flex-col items-center gap-4 text-center">
            {content.heading && <h2 className="text-2xl font-semibold sm:text-3xl">{content.heading}</h2>}
            {content.subheading && <p className="text-muted-foreground">{content.subheading}</p>}
            {content.ctaLabel && content.ctaHref && (
              <Link href={content.ctaHref} className={buttonVariants({ size: "lg" })}>
                {content.ctaLabel}
              </Link>
            )}
          </div>
        </section>
      );
    }

    case "Testimonials": {
      const content = parse<TestimonialsContent>(section.contentJson);
      const items = content.items ?? [];
      if (items.length === 0) return null;
      return (
        <section className="px-4 py-12 sm:py-16">
          <div className="mx-auto grid max-w-5xl gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {items.map((item, i) => (
              <Card key={i}>
                <CardContent className="space-y-3 pt-6">
                  <p className="text-sm italic text-muted-foreground">&ldquo;{item.quote}&rdquo;</p>
                  <p className="text-sm font-medium">{item.author}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </section>
      );
    }

    case "ImageGallery": {
      const content = parse<ImageGalleryContent>(section.contentJson);
      const images = content.images ?? [];
      if (images.length === 0) return null;
      return (
        <section className="px-4 py-12">
          <div className="mx-auto grid max-w-5xl grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4">
            {images.map((image, i) => (
              <div key={i} className="relative aspect-square overflow-hidden rounded-lg bg-muted">
                {image.url && <Image src={image.url} alt={image.alt ?? ""} fill className="object-cover" />}
              </div>
            ))}
          </div>
        </section>
      );
    }

    case "PricingTable":
      return (
        <section className="px-4 py-12 sm:py-16">
          <div className="mx-auto max-w-5xl">
            <PricingCards plans={plans ?? []} />
          </div>
        </section>
      );

    default:
      return null;
  }
}
