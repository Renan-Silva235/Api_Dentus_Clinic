using System.ComponentModel.DataAnnotations;

namespace DentusClinic.API.DTOs.Request;

public class EsqueciSenhaRedefinirRequest
{
    [Required(ErrorMessage = "E-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória.")]
    [StringLength(60, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    public string NovaSenha { get; set; } = string.Empty;
}
