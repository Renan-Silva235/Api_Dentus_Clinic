using System.ComponentModel.DataAnnotations;

namespace DentusClinic.API.DTOs.Request;

public class EsqueciSenhaVerificarRequest
{
    [Required(ErrorMessage = "E-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;
}
