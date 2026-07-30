namespace CrewFlow.Domain.Identity;

// Admin gets every feature. Finance and Operational are staff roles scoped to their own
// slice (billing/cashflow vs. schedule/classes/instructors). Coach is scoped further still -
// a coach only sees/manages their own assigned classes, not the whole schedule. Member is the
// single community-facing role - no separate "student"/trial role; a member with no active
// subscription simply books drop-in via credit packs.
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Finance = "Finance";
    public const string Operational = "Operational";
    public const string Coach = "Coach";
    public const string Member = "Member";

    public static readonly string[] All = [Admin, Finance, Operational, Coach, Member];
    public static readonly string[] StaffRoles = [Admin, Finance, Operational, Coach];
}
