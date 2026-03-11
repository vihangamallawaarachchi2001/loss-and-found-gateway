using Grpc.Core;
using Grpc.Net.Client;
using Lostfound.V1;
using Contracts = gateway.Contracts;

namespace gateway.Services;

public sealed class GrpcBackendClient
{
    private readonly string _backendAddress;

    public GrpcBackendClient(IConfiguration configuration)
    {
        _backendAddress = configuration["Grpc:BackendAddress"] ?? "http://localhost:50051";
    }

    private GrpcChannel CreateChannel()
    {
        return GrpcChannel.ForAddress(_backendAddress);
    }

    public async Task<Contracts.AuthResponseDto> SignupAsync(Contracts.SignupRequest request, CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new AuthService.AuthServiceClient(channel);
        var response = await client.SignupAsync(new Lostfound.V1.SignupRequest
        {
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName
        }, cancellationToken: cancellationToken);

        return new Contracts.AuthResponseDto(response.UserId, response.Email, response.FullName, response.Token);
    }

    public async Task<Contracts.AuthResponseDto> LoginAsync(Contracts.LoginRequest request, CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new AuthService.AuthServiceClient(channel);
        var response = await client.LoginAsync(new Lostfound.V1.LoginRequest
        {
            Email = request.Email,
            Password = request.Password,
        }, cancellationToken: cancellationToken);

        return new Contracts.AuthResponseDto(response.UserId, response.Email, response.FullName, response.Token);
    }

    public async Task<Contracts.OperationResultDto> ForgotPasswordAsync(Contracts.ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new AuthService.AuthServiceClient(channel);
        var response = await client.ForgotPasswordAsync(new Lostfound.V1.ForgotPasswordRequest
        {
            Email = request.Email,
        }, cancellationToken: cancellationToken);

        return new Contracts.OperationResultDto(response.Success, response.Message);
    }

    public async Task<Contracts.OperationResultDto> ResetPasswordAsync(Contracts.ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new AuthService.AuthServiceClient(channel);
        var response = await client.ResetPasswordAsync(new Lostfound.V1.ResetPasswordRequest
        {
            Email = request.Email,
            Otp = request.Otp,
            NewPassword = request.NewPassword,
        }, cancellationToken: cancellationToken);

        return new Contracts.OperationResultDto(response.Success, response.Message);
    }

    public async Task<Contracts.ItemDto> CreateLostItemAsync(Contracts.CreateItemRequest request, IEnumerable<string> imagePaths, CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new ItemService.ItemServiceClient(channel);
        var grpcRequest = new Lostfound.V1.CreateItemRequest
        {
            UserId = request.UserId,
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            Location = request.Location,
            EventDate = request.EventDate,
        };
        grpcRequest.ImagePaths.AddRange(imagePaths);

        var response = await client.CreateLostItemAsync(grpcRequest, cancellationToken: cancellationToken);
        return MapItem(response);
    }

    public async Task<Contracts.ItemDto> CreateFoundItemAsync(Contracts.CreateItemRequest request, IEnumerable<string> imagePaths, CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new ItemService.ItemServiceClient(channel);
        var grpcRequest = new Lostfound.V1.CreateItemRequest
        {
            UserId = request.UserId,
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            Location = request.Location,
            EventDate = request.EventDate,
        };
        grpcRequest.ImagePaths.AddRange(imagePaths);

        var response = await client.CreateFoundItemAsync(grpcRequest, cancellationToken: cancellationToken);
        return MapItem(response);
    }

    public async Task<Contracts.ListItemsResponse> ListLostItemsAsync(int limit, int offset, CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new ItemService.ItemServiceClient(channel);
        var response = await client.ListLostItemsAsync(new ListItemsRequest { Limit = limit, Offset = offset }, cancellationToken: cancellationToken);
        return new Contracts.ListItemsResponse(response.Items.Select(MapItem).ToList(), response.Total);
    }

    public async Task<Contracts.ListItemsResponse> ListFoundItemsAsync(int limit, int offset, CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new ItemService.ItemServiceClient(channel);
        var response = await client.ListFoundItemsAsync(new ListItemsRequest { Limit = limit, Offset = offset }, cancellationToken: cancellationToken);
        return new Contracts.ListItemsResponse(response.Items.Select(MapItem).ToList(), response.Total);
    }

    public async Task<Contracts.ItemDto> GetItemAsync(string id, CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new ItemService.ItemServiceClient(channel);
        var response = await client.GetItemAsync(new GetItemRequest { Id = id }, cancellationToken: cancellationToken);
        return MapItem(response);
    }

    public async Task<IReadOnlyList<Contracts.OwnerAlertDto>> ListOwnerAlertsAsync(string ownerUserId, CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new MatchService.MatchServiceClient(channel);
        var response = await client.ListOwnerAlertsAsync(new ListOwnerAlertsRequest { OwnerUserId = ownerUserId }, cancellationToken: cancellationToken);
        return response.Alerts
            .Select(alert => new Contracts.OwnerAlertDto(alert.LostItemId, alert.FoundItemId, alert.TextScore, alert.ImageScore, alert.Confidence, alert.Status))
            .ToList();
    }

    public async Task<Contracts.OperationResultDto> AcceptMatchAsync(string lostItemId, string foundItemId, string decidedByUserId, CancellationToken cancellationToken)
    {
        return await DecideAsync("accept", lostItemId, foundItemId, decidedByUserId, cancellationToken);
    }

    public async Task<Contracts.OperationResultDto> RejectMatchAsync(string lostItemId, string foundItemId, string decidedByUserId, CancellationToken cancellationToken)
    {
        return await DecideAsync("reject", lostItemId, foundItemId, decidedByUserId, cancellationToken);
    }

    public async Task<Contracts.OperationResultDto> MarkClaimedAsync(string lostItemId, string foundItemId, string decidedByUserId, CancellationToken cancellationToken)
    {
        return await DecideAsync("claim", lostItemId, foundItemId, decidedByUserId, cancellationToken);
    }

    private async Task<Contracts.OperationResultDto> DecideAsync(string mode, string lostItemId, string foundItemId, string decidedByUserId, CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new MatchService.MatchServiceClient(channel);
        var request = new Lostfound.V1.MatchDecisionRequest
        {
            LostItemId = lostItemId,
            FoundItemId = foundItemId,
            DecidedByUserId = decidedByUserId,
        };

        OperationResult response = mode switch
        {
            "accept" => await client.AcceptMatchAsync(request, cancellationToken: cancellationToken),
            "reject" => await client.RejectMatchAsync(request, cancellationToken: cancellationToken),
            _ => await client.MarkClaimedAsync(request, cancellationToken: cancellationToken),
        };

        return new Contracts.OperationResultDto(response.Success, response.Message);
    }

    public async Task<Contracts.ProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new ProfileService.ProfileServiceClient(channel);
        var response = await client.GetProfileAsync(new GetProfileRequest { UserId = userId }, cancellationToken: cancellationToken);
        return new Contracts.ProfileDto(response.UserId, response.Email, response.FullName, response.Phone, response.AvatarPath);
    }

    public async Task<Contracts.ProfileDto> UpdateProfileAsync(string userId, Contracts.UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new ProfileService.ProfileServiceClient(channel);
        var response = await client.UpdateProfileAsync(new Lostfound.V1.UpdateProfileRequest
        {
            UserId = userId,
            FullName = request.FullName,
            Phone = request.Phone,
            AvatarPath = request.AvatarPath,
        }, cancellationToken: cancellationToken);
        return new Contracts.ProfileDto(response.UserId, response.Email, response.FullName, response.Phone, response.AvatarPath);
    }

    private static Contracts.ItemDto MapItem(ItemResponse response)
    {
        return new Contracts.ItemDto(
            response.Id,
            response.UserId,
            response.ItemType,
            response.Title,
            response.Description,
            response.Category,
            response.Location,
            response.EventDate,
            response.Status,
            response.ImagePaths.ToList(),
            response.TextScore,
            response.ImageScore,
            response.Confidence,
            response.CreatedAt);
    }
}
