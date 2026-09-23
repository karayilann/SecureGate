namespace SecureGate.Application.DTOs;

public class ApiKeyListItemDto
{
    public Guid Id { get; set; }
    public string MaskedKeyValue { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
