using gateway.Contracts;
using gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace gateway.Controllers;

[ApiController]
[Route("api/v1/profile")]
public sealed class ProfileController : ControllerBase
{
    private readonly GrpcBackendClient _backendClient;

    public ProfileController(GrpcBackendClient backendClient)
    {
        _backendClient = backendClient;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> GetMe([FromQuery] string userId, CancellationToken cancellationToken)
    {
        return Ok(await _backendClient.GetProfileAsync(userId, cancellationToken));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ProfileDto>> UpdateMe([FromQuery] string userId, [FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _backendClient.UpdateProfileAsync(userId, request, cancellationToken));
    }
}
