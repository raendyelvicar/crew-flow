using CrewFlow.Application.Billing;
using CrewFlow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrewFlow.Api.Controllers;

[ApiController]
[Route("api/v1/webhooks/stripe")]
[AllowAnonymous]
public class StripeWebhookController : ControllerBase
{
    private readonly IStripeService _stripeService;
    private readonly StripeWebhookProcessingService _processingService;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        IStripeService stripeService, StripeWebhookProcessingService processingService, ILogger<StripeWebhookController> logger)
    {
        _stripeService = stripeService;
        _processingService = processingService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(ct);
        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();

        StripeWebhookEvent evt;
        try
        {
            evt = _stripeService.ParseWebhookEvent(payload, signatureHeader);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Rejected Stripe webhook: signature/payload verification failed.");
            return BadRequest();
        }

        await _processingService.ProcessAsync(evt, ct);
        return Ok();
    }
}
