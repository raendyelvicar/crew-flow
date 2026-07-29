using CrewFlow.Api.Common;
using CrewFlow.Application.Bookings;
using CrewFlow.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrewFlow.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly BookingService _bookingService;
    private readonly CrewFlow.Application.Members.MemberService _memberService;

    public BookingsController(BookingService bookingService, CrewFlow.Application.Members.MemberService memberService)
    {
        _bookingService = bookingService;
        _memberService = memberService;
    }

    [HttpPost("bookings")]
    [Authorize(Policy = PolicyNames.MemberOnly)]
    public async Task<ActionResult<BookingResponse>> Book(CreateBookingRequest request, CancellationToken ct)
        => Ok(await _bookingService.CreateBookingAsync(request, ct));

    [HttpPost("bookings/{id:guid}/cancel")]
    [Authorize(Policy = PolicyNames.MemberOnly)]
    public async Task<ActionResult<BookingResponse>> Cancel(Guid id, CancellationToken ct)
        => Ok(await _bookingService.CancelBookingAsync(id, ct));

    [HttpGet("bookings/me")]
    [Authorize(Policy = PolicyNames.MemberOnly)]
    public async Task<ActionResult<IReadOnlyList<MyBookingResponse>>> MyBookings(CancellationToken ct)
    {
        var me = await _memberService.GetByUserIdAsync(this.GetUserId(), ct);
        return Ok(await _bookingService.GetMemberBookingsAsync(me.Id, ct));
    }

    [HttpPost("bookings/{id:guid}/attendance")]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<BookingResponse>> MarkAttendance(Guid id, MarkAttendanceRequest request, CancellationToken ct)
        => Ok(await _bookingService.MarkAttendanceAsync(id, request, ct));

    [HttpPost("class-reviews")]
    [Authorize(Policy = PolicyNames.MemberOnly)]
    public async Task<ActionResult<ClassReviewResponse>> CreateReview(CreateClassReviewRequest request, CancellationToken ct)
        => Ok(await _bookingService.CreateReviewAsync(request, ct));

    [HttpGet("instructors/{instructorUserId:guid}/class-reviews")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ClassReviewResponse>>> InstructorReviews(Guid instructorUserId, CancellationToken ct)
        => Ok(await _bookingService.GetReviewsForInstructorAsync(instructorUserId, ct));
}
