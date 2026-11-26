using DentusClinic.API.Utils;

namespace DentusClinic.API.Models;

public class Prontuario
{
    public int Id { get; set; }

    public int IdPaciente { get; set; }
    public Paciente Paciente { get; set; } = null!;

    public DateOnly DataAbertura { get; set; } = HorarioBrasil.Hoje();

    public DateOnly? DataUltimaAtualizacao { get; set; }

    public ICollection<Planos> Planos { get; set; } = new List<Planos>();
}
