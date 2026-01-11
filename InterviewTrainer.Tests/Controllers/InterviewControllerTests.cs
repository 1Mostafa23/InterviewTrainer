using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using InterviewTrainer.Api.Application.DTOs;
using InterviewTrainer.Api.Application.Interfaces;
using InterviewTrainer.Api.Controllers;
using InterviewTrainer.Api.Domain;

namespace InterviewTrainer.Tests.Controllers;

/// <summary>
/// Тесты для InterviewController.
/// Здесь мы тестируем HTTP-контракт контроллера (коды ответов, формат данных).
/// Используем Moq для мокирования сервиса - контроллер не должен знать о реальной реализации.
/// </summary>
public class InterviewControllerTests
{
    private Mock<IInterviewService> CreateMockService()
    {
        return new Mock<IInterviewService>();
    }

    [Fact]
    public async Task StartSession_Should_Return_Created_When_Success()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new InterviewController(mockService.Object);
        var userId = Guid.NewGuid();
        var request = new StartSessionRequest { UserId = userId };

        var expectedDto = new InterviewSessionDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = SessionStatus.Started.Value,
            CreatedAt = DateTime.UtcNow
        };

        // Настраиваем мок: сервис вернет DTO
        mockService.Setup(s => s.StartSessionAsync(request))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await controller.StartSession(request);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(expectedDto, createdAtResult.Value);
        Assert.Equal(nameof(controller.GetSession), createdAtResult.ActionName);
    }

    [Fact]
    public async Task StartSession_Should_Return_BadRequest_When_DomainException()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new InterviewController(mockService.Object);
        var request = new StartSessionRequest { UserId = Guid.NewGuid() };

        // Настраиваем мок: сервис выбросит DomainException
        mockService.Setup(s => s.StartSessionAsync(request))
            .ThrowsAsync(new DomainException("Ошибка валидации"));

        // Act
        var result = await controller.StartSession(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetSession_Should_Return_Ok_When_Session_Exists()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new InterviewController(mockService.Object);
        var sessionId = Guid.NewGuid();

        var expectedDto = new InterviewSessionDto
        {
            Id = sessionId,
            UserId = Guid.NewGuid(),
            Status = SessionStatus.Started.Value,
            CreatedAt = DateTime.UtcNow
        };

        mockService.Setup(s => s.GetByIdAsync(sessionId))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await controller.GetSession(sessionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedDto, okResult.Value);
    }

    [Fact]
    public async Task GetSession_Should_Return_NotFound_When_Session_Not_Exists()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new InterviewController(mockService.Object);
        var sessionId = Guid.NewGuid();

        // Настраиваем мок: сервис вернет null
        mockService.Setup(s => s.GetByIdAsync(sessionId))
            .ReturnsAsync((InterviewSessionDto?)null);

        // Act
        var result = await controller.GetSession(sessionId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task StartInterview_Should_Return_Ok_When_Success()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new InterviewController(mockService.Object);
        var sessionId = Guid.NewGuid();

        var expectedDto = new InterviewSessionDto
        {
            Id = sessionId,
            UserId = Guid.NewGuid(),
            Status = SessionStatus.InProgress.Value,
            CreatedAt = DateTime.UtcNow
        };

        mockService.Setup(s => s.StartInterviewAsync(sessionId))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await controller.StartInterview(sessionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedDto, okResult.Value);
    }

    [Fact]
    public async Task StartInterview_Should_Return_BadRequest_When_DomainException()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new InterviewController(mockService.Object);
        var sessionId = Guid.NewGuid();

        mockService.Setup(s => s.StartInterviewAsync(sessionId))
            .ThrowsAsync(new DomainException("Сессия не найдена"));

        // Act
        var result = await controller.StartInterview(sessionId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task CompleteSession_Should_Return_Ok_When_Success()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new InterviewController(mockService.Object);
        var sessionId = Guid.NewGuid();

        var request = new CompleteSessionRequest
        {
            Score = 85,
            Summary = "Хорошее интервью",
            Tips = "Продолжай развиваться"
        };

        var expectedDto = new InterviewSessionDto
        {
            Id = sessionId,
            UserId = Guid.NewGuid(),
            Status = SessionStatus.Completed.Value,
            CreatedAt = DateTime.UtcNow,
            FinishedAt = DateTime.UtcNow,
            Score = request.Score,
            Summary = request.Summary,
            Tips = request.Tips
        };

        mockService.Setup(s => s.CompleteSessionAsync(sessionId, request))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await controller.CompleteSession(sessionId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedDto, okResult.Value);
    }

    [Fact]
    public async Task CompleteSession_Should_Return_BadRequest_When_DomainException()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new InterviewController(mockService.Object);
        var sessionId = Guid.NewGuid();

        var request = new CompleteSessionRequest
        {
            Score = 85,
            Summary = "Summary",
            Tips = "Tips"
        };

        mockService.Setup(s => s.CompleteSessionAsync(sessionId, request))
            .ThrowsAsync(new DomainException("Сессия не найдена"));

        // Act
        var result = await controller.CompleteSession(sessionId, request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
}

