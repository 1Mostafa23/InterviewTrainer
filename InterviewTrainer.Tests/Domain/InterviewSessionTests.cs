using Xunit;
using InterviewTrainer.Api.Domain;

namespace InterviewTrainer.Tests.Domain;

/// <summary>
/// Тесты для доменной модели InterviewSession.
/// Эти тесты проверяют бизнес-логику и правила домена БЕЗ зависимостей от БД или других слоев.
/// </summary>
public class InterviewSessionTests
{
    [Fact]
    public void Should_Not_Allow_Starting_Already_Completed_Session()
    {
        // Arrange (настройка всего необходимого, подготовка)
        var session = new InterviewSession(Guid.NewGuid());
        
        // Act (действие с тем что мы хотим проверить)
        session.Start();
        session.Complete(10, "summary", "tips");
        
        // Assert (проверяем, что код работает правильно)
        // Ожидаем исключение при попытке запустить уже завершенную сессию
        Assert.Throws<DomainException>(() => session.Start());
    }

    [Fact]
    public void Should_Create_Session_With_Created_Status()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Act
        var session = new InterviewSession(userId);
        
        // Assert
        Assert.Equal(userId, session.UserId);
        Assert.Equal(SessionStatus.Started.Value, session.Status.Value);
        Assert.NotEqual(default(DateTime), session.CreatedAt);
    }

    [Fact]
    public void Should_Transition_From_Started_To_InProgress()
    {
        // Arrange
        var session = new InterviewSession(Guid.NewGuid());
        
        // Act
        session.Start();
        
        // Assert
        Assert.Equal(SessionStatus.InProgress.Value, session.Status.Value);
    }

    [Fact]
    public void Should_Complete_Session_With_All_Details()
    {
        // Arrange
        var session = new InterviewSession(Guid.NewGuid());
        session.Start();
        var score = 85;
        var summary = "Отличное интервью";
        var tips = "Продолжай в том же духе";
        
        // Act
        session.Complete(score, summary, tips);
        
        // Assert
        Assert.Equal(SessionStatus.Completed.Value, session.Status.Value);
        Assert.Equal(score, session.Score);
        Assert.Equal(summary, session.Summary);
        Assert.Equal(tips, session.Tips);
        Assert.NotNull(session.FinishedAt);
    }

    [Fact]
    public void Should_Not_Allow_Completing_Not_Started_Session()
    {
        // Arrange
        var session = new InterviewSession(Guid.NewGuid());
        // Сессия создана, но не запущена (статус Started, не InProgress)
        
        // Act & Assert
        // Нельзя завершить сессию, которая не была запущена
        Assert.Throws<DomainException>(() => 
            session.Complete(10, "summary", "tips"));
    }
}

