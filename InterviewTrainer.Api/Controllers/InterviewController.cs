using Microsoft.AspNetCore.Mvc;
using InterviewTrainer.Api.Application.DTOs;
using InterviewTrainer.Api.Application.Interfaces;
using InterviewTrainer.Api.Domain;

namespace InterviewTrainer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterviewController : ControllerBase
{
    private readonly IInterviewService _interviewService;

    public InterviewController(IInterviewService interviewService)
    {
        _interviewService = interviewService;
    }

    [HttpPost("Start")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request)
    {
        try
        {
            var sessionDto = await _interviewService.StartSessionAsync(request);
            return CreatedAtAction(nameof(GetSession), new { id = sessionDto.Id }, sessionDto);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSession(Guid id)
    {
        var sessionDto = await _interviewService.GetByIdAsync(id);
        if (sessionDto == null)
        {
            return NotFound(new { error = $"Сессия с ID {id} не найдена" });
        }
        return Ok(sessionDto);
    }

    [HttpPatch("{id}/start")]
    public async Task<IActionResult> StartInterview(Guid id)
    {
        try
        {
            var sessionDto = await _interviewService.StartInterviewAsync(id);
            return Ok(sessionDto);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> CompleteSession(Guid id, [FromBody] CompleteSessionRequest request)
    {
        try
        {
            var sessionDto = await _interviewService.CompleteSessionAsync(id, request);
            return Ok(sessionDto);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}