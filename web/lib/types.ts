// Mirrors the response shapes returned by the CrewFlow .NET API (see api/src/CrewFlow.Application/*/Dtos).

export type MemberStatus = "Active" | "Inactive" | "Archived";
export type SkillLevel = "Beginner" | "Intermediate" | "Advanced";

export type MemberDanceStyle = {
  danceStyleId: string;
  danceStyleName: string;
  skillLevel: SkillLevel;
};

export type Member = {
  id: string;
  userId?: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  dateOfBirth?: string;
  status: MemberStatus;
  joinedAtUtc: string;
  bio?: string;
  avatarUrl?: string;
  instagramHandle?: string;
  tikTokHandle?: string;
  websiteUrl?: string;
  isProfilePublic: boolean;
  notes?: string;
  danceStyles: MemberDanceStyle[];
};

export type MemberDirectoryEntry = {
  id: string;
  firstName: string;
  lastName: string;
  bio?: string;
  avatarUrl?: string;
  danceStyles: MemberDanceStyle[];
};

export type DanceStyle = {
  id: string;
  name: string;
  isActive: boolean;
};

export type ClassType = {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
};

export type Activity = {
  id: string;
  name: string;
  description?: string;
  classGenreId: string;
  classGenreName: string;
  classTypeId: string;
  classTypeName: string;
  defaultCapacity: number;
  defaultDurationMinutes: number;
  isActive: boolean;
};

export type ClassSchedule = {
  id: string;
  activityId: string;
  activityName: string;
  instructorUserId: string;
  instructorName: string;
  dayOfWeek: string;
  startTimeLocal: string;
  durationMinutes: number;
  capacity: number;
  timezone: string;
  effectiveFromDate: string;
  effectiveToDate?: string;
  isActive: boolean;
};

export type OccurrenceStatus = "Scheduled" | "Cancelled" | "Completed";

export type ClassOccurrence = {
  id: string;
  classScheduleId: string;
  activityId: string;
  activityName: string;
  instructorUserId: string;
  instructorName: string;
  startAtUtc: string;
  endAtUtc: string;
  capacity: number;
  bookedCount: number;
  waitlistCount: number;
  status: OccurrenceStatus;
  cancellationReason?: string;
};

export type BookingStatus = "Booked" | "Waitlisted" | "Cancelled" | "Attended" | "NoShow";
export type BookingPaymentMethod = "Subscription" | "Credit" | "Complimentary";

export type Booking = {
  id: string;
  classOccurrenceId: string;
  memberId: string;
  status: BookingStatus;
  paymentMethod: BookingPaymentMethod;
  bookedAtUtc: string;
  waitlistPosition?: number;
  cancelledAtUtc?: string;
};

export type MyBooking = {
  id: string;
  classOccurrenceId: string;
  activityName: string;
  startAtUtc: string;
  status: BookingStatus;
  waitlistPosition?: number;
};

export type RosterEntry = {
  bookingId: string;
  memberId: string;
  memberName: string;
  status: BookingStatus;
  waitlistPosition?: number;
  bookedAtUtc: string;
};

export type BillingInterval = "Monthly" | "Annual";

export type MembershipPlan = {
  id: string;
  name: string;
  description?: string;
  billingInterval: BillingInterval;
  priceCents: number;
  currency: string;
  isActive: boolean;
  sortOrder: number;
};

export type SubscriptionStatus =
  | "Trialing"
  | "Active"
  | "PastDue"
  | "Canceled"
  | "Unpaid"
  | "Incomplete"
  | "IncompleteExpired";

export type Subscription = {
  id: string;
  memberId: string;
  membershipPlanId: string;
  planName: string;
  status: SubscriptionStatus;
  currentPeriodStartUtc?: string;
  currentPeriodEndUtc?: string;
  cancelAtPeriodEnd: boolean;
};

export type CreditPack = {
  id: string;
  name: string;
  description?: string;
  creditCount: number;
  priceCents: number;
  currency: string;
  expiryDays?: number;
  isActive: boolean;
};

export type CreditPackPurchase = {
  id: string;
  memberId: string;
  creditPackId: string;
  creditPackName: string;
  creditsRemaining: number;
  purchasedAtUtc: string;
  expiresAtUtc?: string;
  status: "Active" | "Expired" | "Depleted";
};

export type CashflowSource = "StripeCharge" | "StripeInvoice" | "ManualCash" | "ManualCard" | "Other";
export type CashflowCategory = "Membership" | "CreditPack" | "DropIn" | "Merchandise" | "Other";

export type CashflowEntry = {
  id: string;
  memberId?: string;
  memberName?: string;
  amount: number;
  currency: string;
  source: CashflowSource;
  category: CashflowCategory;
  description?: string;
  referenceStripeObjectId?: string;
  occurredAtUtc: string;
  reconciliationStatus: "Unreconciled" | "Reconciled" | "Disputed";
};

export type CashflowSummary = {
  totalIncome: number;
  totalRefunds: number;
  netAmount: number;
  entryCount: number;
  byCategory: Record<string, number>;
};

export type InstructorDanceStyle = {
  danceStyleId: string;
  danceStyleName: string;
};

export type Instructor = {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  bio?: string;
  avatarUrl?: string;
  yearsExperience?: number;
  instagramHandle?: string;
  websiteUrl?: string;
  isActive: boolean;
  danceStyles: InstructorDanceStyle[];
};

export type ApiProblem = {
  title?: string;
  status?: number;
  detail?: string;
  errors?: string[];
};
