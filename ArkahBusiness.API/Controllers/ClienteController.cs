using ArkahBusiness.API.DTOs;
using ArkahBusiness.API.Extensions;
using ArkahBusiness.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArkahBusiness.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClienteController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly ILogger<ClienteController> _logger;

    public ClienteController(IClienteService clienteService, ILogger<ClienteController> logger)
    {
        _clienteService = clienteService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CriarCliente([FromBody] CreateClienteRequest request)
    {
        int empresaId = User.GetEmpresaId();

        _logger.LogInformation("Recebida requisição POST /api/cliente. EmpresaId: {EmpresaId}", empresaId);

        var response = await _clienteService.CriarClienteAsync(request, empresaId);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> ListarClientes()
    {
        int empresaId = User.GetEmpresaId();
        var clientes = await _clienteService.ListarClientesAsync(empresaId);
        return Ok(clientes);
    }
}