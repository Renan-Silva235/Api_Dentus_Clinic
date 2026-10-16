using DentusClinic.API.Models;

namespace DentusClinic.API.Repositories.Interfaces;

public interface IDispositivoRepository
{
    Task<IEnumerable<Dispositivo>> ListarAtivosPorPacienteAsync(int idPaciente);
    Task<Dispositivo?> BuscarPorTokenAsync(string tokenPush);

    Task AdicionarAsync(Dispositivo dispositivo);
    Task AtualizarAsync(Dispositivo dispositivo);
}
