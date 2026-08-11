namespace ArkahBusiness.API.Models;

public class Empresa
{
    public int Id { get; set; } 
    public string NomeFantasia { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public string? MensagemLembreteWhatsApp { get; set; }
    public bool Ativo { get; set; } = true;


}