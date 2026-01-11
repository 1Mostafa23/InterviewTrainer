namespace InterviewTrainer.Api.Application.DTOs;

public record InterviewSessionDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? FinishedAt { get; init; }
    public int? Score { get; init; }
    public string? Summary { get; init; }
    public string? Tips { get; init; }
}

