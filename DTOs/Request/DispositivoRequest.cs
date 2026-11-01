using System.ComponentModel.DataAnnotations;

namespace DentusClinic.API.DTOs.Request;

public class DispositivoRequest
{
    [Required(ErrorMessage = "Token do dispositivo é obrigatório.")]
    public string TokenPush { get; set; } = string.Empty;

    [Required(ErrorMessage = "Plataforma é obrigatória.")]
    [RegularExpression("^(Android|iOS)$", ErrorMessage = "Plataforma inválida. Valores aceitos: Android, iOS")]
    public string Plataforma { get; set; } = string.Empty;
}
