using gateway.Contracts;
using gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace gateway.Controllers;

[ApiController]
[Route("api/v1/matches")]
public sealed class MatchesController : ControllerBase
{
    private readonly GrpcBackendClient _backendClient;

    public MatchesController(GrpcBackendClient backendClient)
    {
        _backendClient = backendClient;
    }

    [HttpGet("owner-alerts")]
    public async Task<ActionResult<IReadOnlyList<OwnerAlertDto>>> ListOwnerAlerts([FromQuery] string ownerUserId, CancellationToken cancellationToken)
    {
        return Ok(await _backendClient.ListOwnerAlertsAsync(ownerUserId, cancellationToken));
    }

    [HttpPost("{lostId}/{foundId}/accept")]
    public async Task<ActionResult<OperationResultDto>> Accept(string lostId, string foundId, [FromBody] MatchDecisionRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _backendClient.AcceptMatchAsync(lostId, foundId, request.DecidedByUserId, cancellationToken));
    }

    [HttpPost("{lostId}/{foundId}/reject")]
    public async Task<ActionResult<OperationResultDto>> Reject(string lostId, string foundId, [FromBody] MatchDecisionRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _backendClient.RejectMatchAsync(lostId, foundId, request.DecidedByUserId, cancellationToken));
    }

    [HttpPost("{lostId}/{foundId}/claim")]
    public async Task<ActionResult<OperationResultDto>> Claim(string lostId, string foundId, [FromBody] MatchDecisionRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _backendClient.MarkClaimedAsync(lostId, foundId, request.DecidedByUserId, cancellationToken));
    }
}
