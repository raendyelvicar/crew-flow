import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { buttonVariants } from "@/components/ui/button";
import Link from "next/link";
import { formatMoney } from "@/lib/money";
import type { CreditPack } from "@/lib/types";

export function CreditPackCards({ packs }: { packs: CreditPack[] }) {
  const activePacks = packs.filter((pack) => pack.isActive);

  if (activePacks.length === 0) {
    return null;
  }

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {activePacks.map((pack) => (
        <Card key={pack.id} className="flex flex-col">
          <CardHeader>
            <CardTitle>{pack.name}</CardTitle>
            <CardDescription>
              {pack.creditCount} classes
              {pack.expiryDays ? ` - expires in ${pack.expiryDays} days` : ""}
            </CardDescription>
          </CardHeader>
          <CardContent className="flex-1">
            <p className="text-3xl font-bold">{formatMoney(pack.priceAmount, pack.currency)}</p>
          </CardContent>
          <CardFooter>
            <Link href="/register" className={buttonVariants({ className: "w-full" })}>
              Join now
            </Link>
          </CardFooter>
        </Card>
      ))}
    </div>
  );
}
