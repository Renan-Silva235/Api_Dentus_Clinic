namespace DentusClinic.API.DTOs.Response;

public class PacienteResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }
    public string Endereco { get; set; } = string.Empty;
    public bool Ativo { get; set; }

    /// <summary>
    /// Senha temporária em texto puro — preenchida só na resposta do cadastro (RN12),
    /// pois depois disso só existe o hash. Null em todas as outras respostas.
    /// </summary>
    public string? SenhaTemporaria { get; set; }
}
