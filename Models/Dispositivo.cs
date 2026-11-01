using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentusClinic.API.Models;

public class Dispositivo
{
    public int Id { get; set; }

    [ForeignKey(nameof(Paciente))]
    public int IdPaciente { get; set; }
    public Paciente Paciente { get; set; } = null!;

    [Required]
    public string TokenPush { get; set; } = string.Empty;

    [Required]
    public string Plataforma { get; set; } = string.Empty; // "Android" ou "iOS"

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public bool Ativo { get; set; } = true;
}
