namespace InterviewTrainer.Api.Application.DTOs;

public record CompleteSessionRequest
{
    public int Score { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string Tips { get; init; } = string.Empty;
}

