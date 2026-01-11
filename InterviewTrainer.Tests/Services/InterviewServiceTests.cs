using Moq;
using Xunit;
using InterviewTrainer.Api.Application.DTOs;
using InterviewTrainer.Api.Application.Interface;
using InterviewTrainer.Api.Application.Services;
using InterviewTrainer.Api.Domain;

namespace InterviewTrainer.Tests.Services;

/// <summary>
/// Тесты для InterviewService.
/// Здесь мы используем Moq для создания моков репозитория.
/// Это позволяет тестировать бизнес-логику сервиса БЕЗ реальной базы данных.
/// </summary>
public class InterviewServiceTests
{
    // Вспомогательный метод для создания мока репозитория
    private Mock<IInterviewRepository> CreateMockRepository()
    {
        return new Mock<IInterviewRepository>();
    }

    [Fact]
    public async Task StartSessionAsync_Should_Create_And_Save_Session()
    {
        // Arrange (подготовка)
        var mockRepository = CreateMockRepository(); // Создаем мок репозитория
        var service = new InterviewService(mockRepository.Object);
        var userId = Guid.NewGuid();
        var request = new StartSessionRequest { UserId = userId };

        // Настраиваем мок: когда вызовут AddAsync, просто ничего не делаем
        // (мок автоматически запоминает, что метод был вызван)
        mockRepository.Setup(r => r.AddAsync(It.IsAny<InterviewSession>()))
            .Returns(Task.CompletedTask);

        // Act (действие)
        var result = await service.StartSessionAsync(request);

        // Assert (проверка)
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(SessionStatus.Started.Value, result.Status);
        
        // Проверяем, что репозиторий был вызван ровно один раз
        mockRepository.Verify(r => r.AddAsync(It.IsAny<InterviewSession>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Dto_When_Session_Exists()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var service = new InterviewService(mockRepository.Object);
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new InterviewSession(userId);

        // Настраиваем мок: когда вызовут GetByIdAsync с нашим sessionId,
        // вернуть нашу сессию
        mockRepository.Setup(r => r.GetByIdAsync(sessionId))
            .ReturnsAsync(session);

        // Act
        var result = await service.GetByIdAsync(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(session.Id, result.Id); // Используем Id сессии, т.к. она создается с новым Guid
        Assert.Equal(userId, result.UserId);
        Assert.Equal(SessionStatus.Started.Value, result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Session_Not_Exists()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var service = new InterviewService(mockRepository.Object);
        var sessionId = Guid.NewGuid();

        // Настраиваем мок: когда вызовут GetByIdAsync, вернуть null
        mockRepository.Setup(r => r.GetByIdAsync(sessionId))
            .ReturnsAsync((InterviewSession?)null);

        // Act
        var result = await service.GetByIdAsync(sessionId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task StartInterviewAsync_Should_Start_Session_And_Save()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var service = new InterviewService(mockRepository.Object);
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new InterviewSession(userId);

        // Настраиваем мок: GetByIdAsync вернет сессию
        mockRepository.Setup(r => r.GetByIdAsync(sessionId))
            .ReturnsAsync(session);
        
        // UpdateAsync просто завершится успешно
        mockRepository.Setup(r => r.UpdateAsync(It.IsAny<InterviewSession>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await service.StartInterviewAsync(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(SessionStatus.InProgress.Value, result.Status);
        
        // Проверяем, что методы репозитория были вызваны
        mockRepository.Verify(r => r.GetByIdAsync(sessionId), Times.Once);
        mockRepository.Verify(r => r.UpdateAsync(It.IsAny<InterviewSession>()), Times.Once);
    }

    [Fact]
    public async Task StartInterviewAsync_Should_Throw_When_Session_Not_Found()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var service = new InterviewService(mockRepository.Object);
        var sessionId = Guid.NewGuid();

        // Настраиваем мок: GetByIdAsync вернет null
        mockRepository.Setup(r => r.GetByIdAsync(sessionId))
            .ReturnsAsync((InterviewSession?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.StartInterviewAsync(sessionId));
        
        Assert.Contains("не найдена", exception.Message);
        
        // Проверяем, что UpdateAsync НЕ был вызван (т.к. сессия не найдена)
        mockRepository.Verify(r => r.UpdateAsync(It.IsAny<InterviewSession>()), Times.Never);
    }

    [Fact]
    public async Task CompleteSessionAsync_Should_Complete_Session_And_Save()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var service = new InterviewService(mockRepository.Object);
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new InterviewSession(userId);
        session.Start(); // Сначала запускаем сессию

        var request = new CompleteSessionRequest
        {
            Score = 90,
            Summary = "Отличное интервью",
            Tips = "Продолжай развиваться"
        };

        mockRepository.Setup(r => r.GetByIdAsync(sessionId))
            .ReturnsAsync(session);
        mockRepository.Setup(r => r.UpdateAsync(It.IsAny<InterviewSession>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await service.CompleteSessionAsync(sessionId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(SessionStatus.Completed.Value, result.Status);
        Assert.Equal(request.Score, result.Score);
        Assert.Equal(request.Summary, result.Summary);
        Assert.Equal(request.Tips, result.Tips);
        Assert.NotNull(result.FinishedAt);
    }

    [Fact]
    public async Task CompleteSessionAsync_Should_Throw_When_Session_Not_Found()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var service = new InterviewService(mockRepository.Object);
        var sessionId = Guid.NewGuid();

        var request = new CompleteSessionRequest
        {
            Score = 90,
            Summary = "Summary",
            Tips = "Tips"
        };

        mockRepository.Setup(r => r.GetByIdAsync(sessionId))
            .ReturnsAsync((InterviewSession?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.CompleteSessionAsync(sessionId, request));
        
        Assert.Contains("не найдена", exception.Message);
    }
}

