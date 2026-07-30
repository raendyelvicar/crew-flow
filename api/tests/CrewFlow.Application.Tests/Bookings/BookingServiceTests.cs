using CrewFlow.Application.Bookings;
using CrewFlow.Application.Common.Exceptions;
using CrewFlow.Application.Tests.TestSupport;
using CrewFlow.Domain.Billing;
using CrewFlow.Domain.Bookings;
using CrewFlow.Domain.Identity;
using CrewFlow.Domain.Members;
using CrewFlow.Domain.Scheduling;
using FluentAssertions;
using Xunit;

namespace CrewFlow.Application.Tests.Bookings;

public class BookingServiceTests : IDisposable
{
    private readonly SqliteTestDbContextFactory _factory = new();

    [Fact]
    public async Task CreateBooking_WaitlistsWhenFull_ThenPromotesWaitlistOnCancellation()
    {
        var memberAId = Guid.NewGuid();
        var memberBId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();

        await using (var db = _factory.CreateContext())
        {
            var plan = new MembershipPlan { Id = Guid.NewGuid(), Name = "Monthly", BillingInterval = BillingInterval.Monthly, PriceAmount = 5000 };
            var genre = new DanceStyle { Id = Guid.NewGuid(), Name = "Salsa" };
            var classType = new ClassType { Id = Guid.NewGuid(), Name = "Regular" };
            var activity = new Activity { Id = Guid.NewGuid(), Name = "Salsa", ClassGenreId = genre.Id, ClassTypeId = classType.Id, DefaultCapacity = 1, DefaultDurationMinutes = 60 };
            var instructorId = Guid.NewGuid();
            var schedule = new ClassSchedule
            {
                Id = Guid.NewGuid(),
                ActivityId = activity.Id,
                InstructorUserId = instructorId,
                DayOfWeek = DayOfWeek.Monday,
                StartTimeLocal = new TimeOnly(18, 0),
                DurationMinutes = 60,
                Capacity = 1,
                Timezone = "UTC",
                EffectiveFromDate = DateOnly.FromDateTime(DateTime.UtcNow),
            };
            var occurrence = new ClassOccurrence
            {
                Id = occurrenceId,
                ClassScheduleId = schedule.Id,
                ActivityId = activity.Id,
                InstructorUserId = instructorId,
                StartAtUtc = DateTime.UtcNow.AddDays(1),
                EndAtUtc = DateTime.UtcNow.AddDays(1).AddHours(1),
                Capacity = 1,
                Status = OccurrenceStatus.Scheduled,
            };

            db.MembershipPlans.Add(plan);
            db.DanceStyles.Add(genre);
            db.ClassTypes.Add(classType);
            db.Members.Add(new Member { Id = memberAId, FirstName = "A", LastName = "A", Email = "a@test.com" });
            db.Members.Add(new Member { Id = memberBId, FirstName = "B", LastName = "B", Email = "b@test.com" });
            db.Users.Add(new ApplicationUser { Id = instructorId, UserName = "instructor@test.com", Email = "instructor@test.com", FirstName = "In", LastName = "Structor" });
            db.Activities.Add(activity);
            db.ClassSchedules.Add(schedule);
            db.ClassOccurrences.Add(occurrence);
            db.Subscriptions.Add(new Subscription { Id = Guid.NewGuid(), MemberId = memberAId, MembershipPlanId = plan.Id, Status = SubscriptionStatus.Active });
            db.Subscriptions.Add(new Subscription { Id = Guid.NewGuid(), MemberId = memberBId, MembershipPlanId = plan.Id, Status = SubscriptionStatus.Active });
            await db.SaveChangesAsync();
        }

        await using var db2 = _factory.CreateContext();
        var service = new BookingService(db2);

        var bookingA = await service.CreateBookingAsync(new CreateBookingRequest(occurrenceId, memberAId));
        bookingA.Status.Should().Be(BookingStatus.Booked);

        var bookingB = await service.CreateBookingAsync(new CreateBookingRequest(occurrenceId, memberBId));
        bookingB.Status.Should().Be(BookingStatus.Waitlisted);
        bookingB.WaitlistPosition.Should().Be(1);

        await service.CancelBookingAsync(bookingA.Id);

        var roster = await service.GetRosterAsync(occurrenceId);
        roster.Should().ContainSingle(r => r.MemberId == memberBId && r.Status == BookingStatus.Booked);
    }

    [Fact]
    public async Task CreateBooking_Throws_WhenMemberHasNoSubscriptionOrCredits()
    {
        var memberId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();

        await using (var db = _factory.CreateContext())
        {
            var genre = new DanceStyle { Id = Guid.NewGuid(), Name = "Salsa" };
            var classType = new ClassType { Id = Guid.NewGuid(), Name = "Regular" };
            var activity = new Activity { Id = Guid.NewGuid(), Name = "Salsa", ClassGenreId = genre.Id, ClassTypeId = classType.Id, DefaultCapacity = 5, DefaultDurationMinutes = 60 };
            var instructorId = Guid.NewGuid();
            var schedule = new ClassSchedule
            {
                Id = Guid.NewGuid(),
                ActivityId = activity.Id,
                InstructorUserId = instructorId,
                DayOfWeek = DayOfWeek.Monday,
                StartTimeLocal = new TimeOnly(18, 0),
                DurationMinutes = 60,
                Capacity = 5,
                Timezone = "UTC",
                EffectiveFromDate = DateOnly.FromDateTime(DateTime.UtcNow),
            };
            var occurrence = new ClassOccurrence
            {
                Id = occurrenceId,
                ClassScheduleId = schedule.Id,
                ActivityId = activity.Id,
                InstructorUserId = instructorId,
                StartAtUtc = DateTime.UtcNow.AddDays(1),
                EndAtUtc = DateTime.UtcNow.AddDays(1).AddHours(1),
                Capacity = 5,
                Status = OccurrenceStatus.Scheduled,
            };

            db.DanceStyles.Add(genre);
            db.ClassTypes.Add(classType);
            db.Members.Add(new Member { Id = memberId, FirstName = "C", LastName = "C", Email = "c@test.com" });
            db.Users.Add(new ApplicationUser { Id = instructorId, UserName = "instructor2@test.com", Email = "instructor2@test.com", FirstName = "In", LastName = "Structor" });
            db.Activities.Add(activity);
            db.ClassSchedules.Add(schedule);
            db.ClassOccurrences.Add(occurrence);
            await db.SaveChangesAsync();
        }

        await using var db2 = _factory.CreateContext();
        var service = new BookingService(db2);

        var act = () => service.CreateBookingAsync(new CreateBookingRequest(occurrenceId, memberId));
        await act.Should().ThrowAsync<ConflictException>();
    }

    public void Dispose() => _factory.Dispose();
}
