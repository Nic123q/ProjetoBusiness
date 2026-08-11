using ArkahBusiness.API.Data;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ArkahBusiness.API.Services;

public interface IWhatsAppService
{
    Task EnviarLembreteAsync(int empresaId, string nomeCliente, string telefone, string dataHoraStr);
}

public class WhatsAppService : IWhatsAppService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly AppDbContext _context;

    public WhatsAppService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<WhatsAppService> logger,
        AppDbContext context)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    public async Task EnviarLembreteAsync(int empresaId, string nomeCliente, string telefone, string dataHoraStr)
    {
        var empresa = await _context.Empresas.FindAsync(empresaId);

        string template = string.IsNullOrWhiteSpace(empresa?.MensagemLembreteWhatsApp)
            ? "Olá {{Nome}}! Passando para lembrar do seu agendamento no dia {{Data}}."
            : empresa.MensagemLembreteWhatsApp;

        string mensagemFinal = template
            .Replace("{{Nome}}", nomeCliente)
            .Replace("{{Data}}", dataHoraStr);

        string baseUrl = _configuration["EvolutionAPI:BaseUrl"];
        string apiKey = _configuration["EvolutionAPI:ApiKey"];
        string instanceName = _configuration["EvolutionAPI:InstanceName"];

        var payload = new
        {
            number = telefone,
            options = new { delay = 1200, presence = "composing" },
            text = mensagemFinal
        };

        var client = _httpClientFactory.CreateClient("EvolutionAPI");
        client.DefaultRequestHeaders.Add("apikey", apiKey);

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation("Iniciando envio de lembrete WhatsApp para {Telefone} (EmpresaId: {EmpresaId})", telefone, empresaId);

            var response = await client.PostAsync($"{baseUrl}/message/sendText/{instanceName}", content);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Falha na Evolution API ao enviar WhatsApp. Status: {StatusCode}, Telefone: {Telefone}, Detalhe: {Erro}",
                    response.StatusCode, telefone, erro);
            }
            else
            {
                _logger.LogInformation("✅ Mensagem enviada com SUCESSO para o telefone {Telefone}", telefone);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔥 Exceção ao tentar enviar WhatsApp para {Telefone}", telefone);
        }
    }
}