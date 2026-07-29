import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { CreateCashflowEntryDialog } from "@/components/create-cashflow-entry-dialog";
import { apiClient } from "@/lib/api-client";
import type { CashflowEntry, CashflowSummary } from "@/lib/types";

function formatMoney(amount: number, currency: string) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: currency.toUpperCase() }).format(amount);
}

export default async function CashflowPage() {
  const now = new Date();
  const from = new Date(now.getFullYear(), now.getMonth(), 1).toISOString();
  const to = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59).toISOString();

  const [summary, entries] = await Promise.all([
    apiClient.get<CashflowSummary>(`/api/v1/cashflow/summary?fromUtc=${from}&toUtc=${to}`),
    apiClient.get<CashflowEntry[]>(`/api/v1/cashflow?fromUtc=${from}&toUtc=${to}`),
  ]);

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Cashflow</h1>
          <p className="text-sm text-muted-foreground">This month</p>
        </div>
        <CreateCashflowEntryDialog />
      </div>

      <div className="grid gap-4 grid-cols-2 sm:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-normal text-muted-foreground">Income</CardTitle>
          </CardHeader>
          <CardContent className="text-xl font-bold">{formatMoney(summary.totalIncome, "usd")}</CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-normal text-muted-foreground">Refunds</CardTitle>
          </CardHeader>
          <CardContent className="text-xl font-bold">{formatMoney(summary.totalRefunds, "usd")}</CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-normal text-muted-foreground">Net</CardTitle>
          </CardHeader>
          <CardContent className="text-xl font-bold">{formatMoney(summary.netAmount, "usd")}</CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-normal text-muted-foreground">Entries</CardTitle>
          </CardHeader>
          <CardContent className="text-xl font-bold">{summary.entryCount}</CardContent>
        </Card>
      </div>

      <div className="overflow-x-auto rounded-lg border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Date</TableHead>
              <TableHead>Member</TableHead>
              <TableHead>Source</TableHead>
              <TableHead>Category</TableHead>
              <TableHead>Description</TableHead>
              <TableHead className="text-right">Amount</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {entries.map((entry) => (
              <TableRow key={entry.id}>
                <TableCell>{new Date(entry.occurredAtUtc).toLocaleDateString()}</TableCell>
                <TableCell>{entry.memberName ?? "-"}</TableCell>
                <TableCell>
                  <Badge variant="secondary">{entry.source}</Badge>
                </TableCell>
                <TableCell>{entry.category}</TableCell>
                <TableCell className="max-w-48 truncate">{entry.description ?? "-"}</TableCell>
                <TableCell className={`text-right font-medium ${entry.amount < 0 ? "text-destructive" : ""}`}>
                  {formatMoney(entry.amount, entry.currency)}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
