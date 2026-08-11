using Hangfire;
using ArkahBusiness.API.Data;
using ArkahBusiness.API.DTOs;
using ArkahBusiness.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ArkahBusiness.API.Services;

public interface IAgendamentoService
{
    Task<AgendamentoResponse> CriarAgendamentoAsync(CreateAgendamentoRequest request, int empresaId);
    Task<List<AgendamentoResponse>> ListarAgendamentosAsync(int empresaId);
}

public class AgendamentoService : IAgendamentoService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AgendamentoService> _logger; 

    public AgendamentoService(AppDbContext context, ILogger<AgendamentoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AgendamentoResponse> CriarAgendamentoAsync(CreateAgendamentoRequest request, int empresaId)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == request.ClienteId && c.EmpresaId == empresaId);

        if (cliente == null)
        {
            _logger.LogWarning("Tentativa de criar agendamento para ClienteId {ClienteId} inválido ou não pertencente à Empresa {EmpresaId}", request.ClienteId, empresaId);
            throw new ArgumentException("Cliente não encontrado ou não pertence a esta empresa.");
        }

        var agendamento = new Agendamento
        {
            ClienteId = request.ClienteId,
            DataHora = request.DataHora,
            Observacao = request.Observacao,
            Status = "Pendente",
            EmpresaId = empresaId
        };

        _context.Agendamentos.Add(agendamento);
        await _context.SaveChangesAsync();

        var dataFormatada = agendamento.DataHora.ToString("dd/MM/yyyy 'às' HH:mm");

        BackgroundJob.Schedule<IWhatsAppService>(
            whatsApp => whatsApp.EnviarLembreteAsync(empresaId, cliente.Nome, cliente.TelefoneWhatsApp, dataFormatada),
            TimeSpan.FromMinutes(1)
        );

        _logger.LogInformation("Agendamento criado e Lembrete programado no Hangfire. ClienteId: {ClienteId}, EmpresaId: {EmpresaId}, DataHora: {DataHora}",
            cliente.Id, empresaId, agendamento.DataHora);

        return new AgendamentoResponse
        {
            Id = agendamento.Id,
            ClienteId = agendamento.ClienteId,
            NomeCliente = cliente.Nome,
            DataHora = agendamento.DataHora,
            Status = agendamento.Status,
            Observacao = agendamento.Observacao
        };
    }

    public async Task<List<AgendamentoResponse>> ListarAgendamentosAsync(int empresaId)
    {
        return await _context.Agendamentos
            .Include(a => a.Cliente)
            .Where(a => a.EmpresaId == empresaId)
            .Select(a => new AgendamentoResponse
            {
                Id = a.Id,
                ClienteId = a.ClienteId,
                NomeCliente = a.Cliente.Nome,
                DataHora = a.DataHora,
                Status = a.Status,
                Observacao = a.Observacao
            })
            .OrderBy(a => a.DataHora)
            .ToListAsync();
    }
}