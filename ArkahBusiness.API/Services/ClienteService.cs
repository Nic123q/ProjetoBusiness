using ArkahBusiness.API.Data;
using ArkahBusiness.API.DTOs;
using ArkahBusiness.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ArkahBusiness.API.Services;

public interface IClienteService
{
    Task<ClienteResponse> CriarClienteAsync(CreateClienteRequest request, int empresaId);
    Task<List<ClienteResponse>> ListarClientesAsync(int empresaId);
}

public class ClienteService : IClienteService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClienteService> _logger;

    public ClienteService(AppDbContext context, ILogger<ClienteService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ClienteResponse> CriarClienteAsync(CreateClienteRequest request, int empresaId)
    {
        _logger.LogInformation("Iniciando cadastro de cliente. Nome: {Nome}, EmpresaId: {EmpresaId}", request.Nome, empresaId);

        var cliente = new Cliente
        {
            Nome = request.Nome,
            TelefoneWhatsApp = request.TelefoneWhatsApp,
            Email = request.Email,
            EmpresaId = empresaId,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cliente cadastrado com sucesso. ClienteId: {ClienteId}, EmpresaId: {EmpresaId}", cliente.Id, empresaId);

        return new ClienteResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            TelefoneWhatsApp = cliente.TelefoneWhatsApp,
            Email = cliente.Email,
            Ativo = cliente.Ativo
        };
    }

    public async Task<List<ClienteResponse>> ListarClientesAsync(int empresaId)
    {
        return await _context.Clientes
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .Select(c => new ClienteResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                TelefoneWhatsApp = c.TelefoneWhatsApp,
                Email = c.Email,
                Ativo = c.Ativo
            })
            .ToListAsync();
    }
}