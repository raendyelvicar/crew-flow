namespace CrewFlow.Application.Common.Security;

// Admin satisfies every policy below (each policy's role set includes Admin).
public static class PolicyNames
{
    public const string AdminOnly = "AdminOnly";

    // Cashflow, membership plans, subscriptions, credit packs.
    public const string FinanceAccess = "FinanceAccess";

    // Activities, class schedules, occurrences, instructors, roster/check-in, members CRUD.
    public const string OperationalAccess = "OperationalAccess";

    // A coach's own dashboard/class list - Admin can see everything, a Coach only their own
    // (enforced in the action, not the policy, since "own" isn't expressible as a role check).
    public const string CoachAccess = "CoachAccess";

    // Roster viewing and attendance check-in: Operational/Admin can act on any class, a Coach
    // only on classes they instruct (ownership checked in the action).
    public const string OperationalOrCoach = "OperationalOrCoach";

    // Any of Admin/Finance/Operational/Coach - e.g. read-only endpoints shared across back-office roles.
    public const string AnyStaff = "AnyStaff";

    public const string MemberOnly = "MemberOnly";
}
