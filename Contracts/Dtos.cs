namespace gateway.Contracts;

public sealed record SignupRequest(string Email, string Password, string FullName);
public sealed record LoginRequest(string Email, string Password);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Email, string Otp, string NewPassword);

public sealed record CreateItemRequest(
    string UserId,
    string Title,
    string Description,
    string Category,
    string Location,
    string EventDate);

public sealed record ItemDto(
    string Id,
    string UserId,
    string ItemType,
    string Title,
    string Description,
    string Category,
    string Location,
    string EventDate,
    string Status,
    IReadOnlyList<string> ImagePaths,
    double TextScore,
    double ImageScore,
    double Confidence,
    string CreatedAt);

public sealed record ListItemsResponse(IReadOnlyList<ItemDto> Items, int Total);
public sealed record MatchDecisionRequest(string DecidedByUserId);

public sealed record OwnerAlertDto(
    string LostItemId,
    string FoundItemId,
    double TextScore,
    double ImageScore,
    double Confidence,
    string Status);

public sealed record UpdateProfileRequest(string FullName, string Phone, string AvatarPath);

public sealed record ProfileDto(
    string UserId,
    string Email,
    string FullName,
    string Phone,
    string AvatarPath);

public sealed record OperationResultDto(bool Success, string Message);

public sealed record AuthResponseDto(string UserId, string Email, string FullName, string Token);
