namespace CrewFlow.Domain.Identity;

// Admin gets every feature. Finance and Operational are the two other staff roles,
// each scoped to their own slice (billing/cashflow vs. schedule/classes/instructors).
// Member is the single community-facing role - no separate "student"/trial role;
// a member with no active subscription simply books drop-in via credit packs.
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Finance = "Finance";
    public const string Operational = "Operational";
    public const string Member = "Member";

    public static readonly string[] All = [Admin, Finance, Operational, Member];
    public static readonly string[] StaffRoles = [Admin, Finance, Operational];
}
