namespace CrewFlow.Application.Common.Security;

// Admin satisfies every policy below (each policy's role set includes Admin).
public static class PolicyNames
{
    public const string AdminOnly = "AdminOnly";

    // Cashflow, membership plans, subscriptions, credit packs.
    public const string FinanceAccess = "FinanceAccess";

    // Activities, class schedules, occurrences, instructors, roster/check-in, members CRUD.
    public const string OperationalAccess = "OperationalAccess";

    // Any of Admin/Finance/Operational - e.g. read-only endpoints shared across back-office roles.
    public const string AnyStaff = "AnyStaff";

    public const string MemberOnly = "MemberOnly";
}
