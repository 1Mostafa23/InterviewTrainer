using InterviewTrainer.Api.Domain;
namespace InterviewTrainer.Api.Application.Interface;

public interface IInterviewRepository
{
    Task<InterviewSession?> GetByIdAsync(Guid id);
    Task AddAsync(InterviewSession session);
    Task UpdateAsync(InterviewSession session);
    Task DeleteAsync(Guid id);
}