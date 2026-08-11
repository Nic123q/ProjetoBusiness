namespace ArkahBusiness.API.DTOs;

public class CreateClienteRequest
{
    public string Nome { get; set; } = string.Empty;
    public string TelefoneWhatsApp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ClienteResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string TelefoneWhatsApp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}