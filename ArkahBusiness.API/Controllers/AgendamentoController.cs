using ArkahBusiness.API.DTOs;
using ArkahBusiness.API.Extensions;
using ArkahBusiness.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArkahBusiness.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AgendamentoController : ControllerBase
{
    private readonly IAgendamentoService _agendamentoService;
    private readonly ILogger<AgendamentoController> _logger; 

    public AgendamentoController(IAgendamentoService agendamentoService, ILogger<AgendamentoController> logger)
    {
        _agendamentoService = agendamentoService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CriarAgendamento([FromBody] CreateAgendamentoRequest request)
    {
        int empresaId = User.GetEmpresaId();

        _logger.LogInformation("Recebida requisição POST /api/agendamento. EmpresaId: {EmpresaId}, ClienteId: {ClienteId}", empresaId, request.ClienteId);

        var response = await _agendamentoService.CriarAgendamentoAsync(request, empresaId);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> ListarAgendamentos()
    {
        int empresaId = User.GetEmpresaId();
        var agendamentos = await _agendamentoService.ListarAgendamentosAsync(empresaId);
        return Ok(agendamentos);
    }
}