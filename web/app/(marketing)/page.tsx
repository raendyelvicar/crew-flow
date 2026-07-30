import Link from "next/link";
import { buttonVariants } from "@/components/ui/button";

export default function HomePage() {
  return (
    <section className="px-4 py-16 sm:py-24">
      <div className="mx-auto flex max-w-3xl flex-col items-center gap-6 text-center">
        <h1 className="text-3xl font-bold tracking-tight sm:text-5xl">Welcome to Crew Flow</h1>
        <p className="max-w-xl text-base text-muted-foreground sm:text-lg">
          A dance studio and community - browse classes, join the crew, and grow your practice.
        </p>
        <div className="flex flex-col gap-3 sm:flex-row">
          <Link href="/register" className={buttonVariants({ size: "lg" })}>
            Join now
          </Link>
          <Link href="/pricing" className={buttonVariants({ size: "lg", variant: "outline" })}>
            View membership
          </Link>
        </div>
      </div>
    </section>
  );
}
