using InterviewTrainer.Api.Application.DTOs;

namespace InterviewTrainer.Api.Application.Interfaces;

public interface IInterviewService
{
    Task<InterviewSessionDto> StartSessionAsync(StartSessionRequest request);
    Task<InterviewSessionDto?> GetByIdAsync(Guid id);
    Task<InterviewSessionDto> StartInterviewAsync(Guid sessionId);
    Task<InterviewSessionDto> CompleteSessionAsync(Guid sessionId, CompleteSessionRequest request);
}

