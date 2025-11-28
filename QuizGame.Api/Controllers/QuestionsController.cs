using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuizGame.Application.DTOs;
using QuizGame.Application.Interfaces;
using QuizGame.Application.Services;

namespace QuizGame.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly ILogger<QuestionsController> _logger;

        public QuestionsController(IQuestionService questionService, ILogger<QuestionsController> logger)
        {
            _questionService = questionService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet()]
        public IActionResult GetAllQuestions()
        {
            _logger.LogInformation("GET /api/questions/getAllQuestions called");
            var questions = _questionService.GetAllQuestions();
            return Ok(questions);
        }
    }
}
