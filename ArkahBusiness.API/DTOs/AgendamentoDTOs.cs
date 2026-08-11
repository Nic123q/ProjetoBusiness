namespace ArkahBusiness.API.DTOs;

public class CreateAgendamentoRequest
{
    public int ClienteId { get; set; }
    public DateTime DataHora { get; set; }
    public string Observacao { get; set; } = string.Empty;
}

public class AgendamentoResponse
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string NomeCliente { get; set; } = string.Empty; 
    public DateTime DataHora { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Observacao { get; set; } = string.Empty;
}