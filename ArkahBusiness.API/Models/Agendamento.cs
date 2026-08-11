namespace ArkahBusiness.API.Models;

 public class Agendamento
 {
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public string Status { get; set; } = "Pendente";
    public string Observacao { get; set; } = string.Empty;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}