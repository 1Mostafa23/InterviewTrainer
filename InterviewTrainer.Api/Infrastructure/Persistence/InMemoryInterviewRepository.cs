using InterviewTrainer.Api.Domain;
using InterviewTrainer.Api.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
namespace InterviewTrainer.Api.Application.Interface;

public class InterviewRepository : IInterviewRepository
{
    private readonly AppDbContext _appDbContext;
    public InterviewRepository(AppDbContext appDbContext) => _appDbContext = appDbContext;

    public async Task AddAsync(InterviewSession session)
    {
        await _appDbContext.interviewSessions.AddAsync(session);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var session = await GetByIdAsync(id);
        if(session != null)
        {
            _appDbContext.Remove(session);
            await _appDbContext.SaveChangesAsync();
        }
    }

    public async Task<InterviewSession?> GetByIdAsync(Guid id) =>
    await _appDbContext.interviewSessions.FindAsync(id);

    public async Task UpdateAsync(InterviewSession session)
    {
        _appDbContext.Update(session);
        await _appDbContext.SaveChangesAsync(); 
    }
}
