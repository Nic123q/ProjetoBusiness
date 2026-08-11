namespace ArkahBusiness.API.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string TelefoneWhatsApp { get; set; } = string.Empty; 
    public string Email { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public bool Ativo { get; set; } = true;

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}