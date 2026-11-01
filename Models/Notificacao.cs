using DentusClinic.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace DentusClinic.API.Models;

public class Notificacao
{
    public int Id { get; set; }

    public int IdConsulta { get; set; }
    public Consulta Consulta { get; set; } = null!;

    [Required]
    public string TipoNotificacao { get; set; } = string.Empty; // ex.: "Confirmação de consulta"

    [Required]
    public StatusEnvioEnum StatusEnvio { get; set; } = StatusEnvioEnum.Pendente;

    public DateTime? DataEnvio { get; set; }

    public int Tentativas { get; set; }
}
