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

        /// <summary>
        /// Retrieves all quiz questions from the system.
        /// </summary>
        /// <returns>
        /// - <c>200 OK</c> with a collection of <see cref="QuestionResponse"/> objects.
        /// - <c>500 Internal Server Error</c> if an unexpected error occurs while fetching the questions.
        /// </returns>
        /// <remarks>
        /// - Calls <see cref="_questionService.GetAllQuestions"/> to retrieve all questions.
        /// - Requires the user to be authenticated via [Authorize].
        /// - Useful for displaying questions in an admin panel or quiz interface.
        /// </remarks>
        [Authorize]
        [HttpGet]
        public IActionResult GetAllQuestions()
        {
            try
            {
                _logger.LogInformation("GET /api/questions called");
                var questions = _questionService.GetAllQuestions();
                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching questions");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
