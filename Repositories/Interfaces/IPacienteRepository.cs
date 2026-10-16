using DentusClinic.API.Models;

namespace DentusClinic.API.Repositories.Interfaces;

public interface IPacienteRepository
{
    Task<IEnumerable<Paciente>> ListarTodosAsync();
    Task<Paciente?> BuscarPorIdAsync(int id);
    Task<Paciente?> BuscarPorLoginIdAsync(int idLogin);
    Task<bool> ExisteCpfAsync(string cpf, int? idExcluido = null);
    Task AdicionarAsync(Paciente paciente);
    Task AtualizarAsync(Paciente paciente);
    Task InativarAsync(Paciente paciente);
}
