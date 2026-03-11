using gateway.Contracts;
using gateway.Services;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using GrpcStatusCode = Grpc.Core.StatusCode;

namespace gateway.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly GrpcBackendClient _backendClient;

    public AuthController(GrpcBackendClient backendClient)
    {
        _backendClient = backendClient;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponseDto>> Signup([FromBody] SignupRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _backendClient.SignupAsync(request, cancellationToken));
        }
        catch (RpcException ex)
        {
            return StatusCode(StatusFromGrpc(ex.StatusCode), new { error = ex.Status.Detail });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _backendClient.LoginAsync(request, cancellationToken));
        }
        catch (RpcException ex)
        {
            return StatusCode(StatusFromGrpc(ex.StatusCode), new { error = ex.Status.Detail });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<OperationResultDto>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _backendClient.ForgotPasswordAsync(request, cancellationToken));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<OperationResultDto>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _backendClient.ResetPasswordAsync(request, cancellationToken));
        }
        catch (RpcException ex)
        {
            return StatusCode(StatusFromGrpc(ex.StatusCode), new { error = ex.Status.Detail });
        }
    }

    private static int StatusFromGrpc(GrpcStatusCode statusCode)
    {
        return statusCode switch
        {
            GrpcStatusCode.NotFound => StatusCodes.Status404NotFound,
            GrpcStatusCode.AlreadyExists => StatusCodes.Status409Conflict,
            GrpcStatusCode.Unauthenticated => StatusCodes.Status401Unauthorized,
            GrpcStatusCode.InvalidArgument => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError,
        };
    }
}
