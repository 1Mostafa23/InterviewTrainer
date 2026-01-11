using InterviewTrainer.Api.Domain;
namespace InterviewTrainer.Api.Domain;


public class InterviewSession // aggregate root
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    
    // Статус меняется только внутри этого класса
    public SessionStatus Status { get; private set; } = SessionStatus.Started;
    
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; private set; }

    public int? Score { get; private set; }
    public string? Summary { get; private set; }
    public string? Tips { get; private set; }

    //! Конструктор (нужен для инициализации сессии)
    public InterviewSession(Guid userId)
    {
        UserId = userId;
    }

    //* Метод для перехода к интервью
    public void Start() => TransitionTo(SessionStatus.InProgress);

    //* метод выполнения самого интервью
    public void Complete(int score, string summary, string tips)
    {
        TransitionTo(SessionStatus.Completed);
        Score = score;
        Summary = summary;
        Tips = tips;
        FinishedAt = DateTime.UtcNow;
    }

    // Общий метод для смены статуса с проверкой логики
    private void TransitionTo(SessionStatus nextStatus)
    {
        if (!Status.CanChangeTo(nextStatus))
        {
            // Здесь мы кинем тот самый DomainException, о котором говорили
            throw new DomainException($"Нельзя сменить статус с {Status.Value} на {nextStatus.Value}");
        }
        Status = nextStatus;
    }
}