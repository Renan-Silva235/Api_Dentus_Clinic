using DentusClinic.API.Models;

namespace DentusClinic.API.Repositories.Interfaces;

public interface IDispositivoRepository
{
    Task<IEnumerable<Dispositivo>> ListarAtivosPorPacienteAsync(int idPaciente);
}
