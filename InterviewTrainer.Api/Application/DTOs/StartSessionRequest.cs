namespace InterviewTrainer.Api.Application.DTOs;

public record StartSessionRequest
{
    public Guid UserId { get; init; }
}

