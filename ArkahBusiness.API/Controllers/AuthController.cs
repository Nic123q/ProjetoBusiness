using ArkahBusiness.API.DTOs;
using ArkahBusiness.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArkahBusiness.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Recebida requisição POST /api/auth/login para o email: {Email}", request.Email);

        var response = await _authService.LoginAsync(request);

        if (response == null)
        {
            _logger.LogWarning("Retornando 401 Unauthorized para o email: {Email}", request.Email);
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        return Ok(response);
    }
}