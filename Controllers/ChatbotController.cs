using Microsoft.AspNetCore.Mvc;
using PetConnect.Services;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly ChatbotService _chatbotService;

    public ChatbotController(ChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("La pregunta no puede estar vacía.");
        }

        var response = await _chatbotService.GenerarRespuestaAsync(request.Question);
        return Ok(new { answer = response });
    }
}

public class ChatRequest
{
    public string Question { get; set; }
}