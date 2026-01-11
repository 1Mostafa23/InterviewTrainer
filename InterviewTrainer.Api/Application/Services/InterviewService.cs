using InterviewTrainer.Api.Application.DTOs;
using InterviewTrainer.Api.Application.Interfaces;
using InterviewTrainer.Api.Application.Interface;
using InterviewTrainer.Api.Domain;

namespace InterviewTrainer.Api.Application.Services;

public class InterviewService : IInterviewService
{
    private readonly IInterviewRepository _repository;

    public InterviewService(IInterviewRepository repository)
    {
        _repository = repository;
    }

    public async Task<InterviewSessionDto> StartSessionAsync(StartSessionRequest request)
    {
        var session = new InterviewSession(request.UserId);
        await _repository.AddAsync(session);

        return MapToDto(session);
    }

    public async Task<InterviewSessionDto?> GetByIdAsync(Guid id)
    {
        var session = await _repository.GetByIdAsync(id);
        return session == null ? null : MapToDto(session);
    }

    public async Task<InterviewSessionDto> StartInterviewAsync(Guid sessionId)
    {
        var session = await _repository.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new DomainException($"Сессия с ID {sessionId} не найдена");
        }

        session.Start();
        await _repository.UpdateAsync(session);

        return MapToDto(session);
    }

    public async Task<InterviewSessionDto> CompleteSessionAsync(Guid sessionId, CompleteSessionRequest request)
    {
        var session = await _repository.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new DomainException($"Сессия с ID {sessionId} не найдена");
        }

        session.Complete(request.Score, request.Summary, request.Tips);
        await _repository.UpdateAsync(session);

        return MapToDto(session);
    }

    private static InterviewSessionDto MapToDto(InterviewSession session)
    {
        return new InterviewSessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            Status = session.Status.Value,
            CreatedAt = session.CreatedAt,
            FinishedAt = session.FinishedAt,
            Score = session.Score,
            Summary = session.Summary,
            Tips = session.Tips
        };
    }
}

