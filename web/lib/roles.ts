// Single source of truth for "where does this user land" - used by the portal, admin, and
// coach layouts so a user who doesn't match one area's required role gets redirected to
// the area they *do* match, rather than bouncing between two layouts that both reject them.
export function resolveHomeRoute(roles: string[] | undefined): string {
  const r = roles ?? [];
  if (r.includes("Admin") || r.includes("Finance") || r.includes("Operational")) return "/admin";
  if (r.includes("Member")) return "/dashboard";
  if (r.includes("Coach")) return "/coach";
  return "/login";
}
