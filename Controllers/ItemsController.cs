using gateway.Contracts;
using gateway.Infrastructure;
using gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace gateway.Controllers;

[ApiController]
[Route("api/v1/items")]
public sealed class ItemsController : ControllerBase
{
    private readonly GrpcBackendClient _backendClient;
    private readonly FileStorage _fileStorage;

    public ItemsController(GrpcBackendClient backendClient, FileStorage fileStorage)
    {
        _backendClient = backendClient;
        _fileStorage = fileStorage;
    }

    [HttpGet("lost")]
    public async Task<ActionResult<ListItemsResponse>> ListLost([FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken cancellationToken = default)
    {
        return Ok(await _backendClient.ListLostItemsAsync(limit, offset, cancellationToken));
    }

    [HttpGet("found")]
    public async Task<ActionResult<ListItemsResponse>> ListFound([FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken cancellationToken = default)
    {
        return Ok(await _backendClient.ListFoundItemsAsync(limit, offset, cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetItem(string id, CancellationToken cancellationToken)
    {
        return Ok(await _backendClient.GetItemAsync(id, cancellationToken));
    }

    [HttpPost("lost")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<ItemDto>> CreateLostItem(
        [FromForm] string userId,
        [FromForm] string title,
        [FromForm] string description,
        [FromForm] string category,
        [FromForm] string location,
        [FromForm] string eventDate,
        [FromForm] List<IFormFile>? photos,
        CancellationToken cancellationToken)
    {
        var imagePaths = await _fileStorage.SaveUploadsAsync(photos, cancellationToken);
        var request = new CreateItemRequest(userId, title, description, category, location, eventDate);
        return Ok(await _backendClient.CreateLostItemAsync(request, imagePaths, cancellationToken));
    }

    [HttpPost("found")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<ItemDto>> CreateFoundItem(
        [FromForm] string userId,
        [FromForm] string title,
        [FromForm] string description,
        [FromForm] string category,
        [FromForm] string location,
        [FromForm] string eventDate,
        [FromForm] List<IFormFile>? photos,
        CancellationToken cancellationToken)
    {
        var imagePaths = await _fileStorage.SaveUploadsAsync(photos, cancellationToken);
        var request = new CreateItemRequest(userId, title, description, category, location, eventDate);
        return Ok(await _backendClient.CreateFoundItemAsync(request, imagePaths, cancellationToken));
    }
}
