using DentusClinic.API.DTOs.Request;
using DentusClinic.API.DTOs.Response;

namespace DentusClinic.API.Services.Interfaces;

public interface IPacienteAppService
{
    Task RegistrarDispositivoAsync(int idPaciente, DispositivoRequest request);
    Task<IEnumerable<PlanosResponse>> ListarTratamentosAsync(int idPaciente);
    Task<PacienteResponse?> BuscarPerfilAsync(int idPaciente);
    Task<PacienteResponse?> AtualizarPerfilAsync(int idPaciente, PacienteEditarRequest request);
    Task AlterarSenhaAsync(int idPaciente, AlterarSenhaRequest request);
    Task<IEnumerable<ConsultaResponse>> ListarConsultasFuturasAsync(int idPaciente);
    Task<IEnumerable<ConsultaResponse>> ListarHistoricoAsync(int idPaciente);
    Task ConfirmarPresencaAsync(int idPaciente, int idConsulta);
    Task RecusarPresencaAsync(int idPaciente, int idConsulta);
    Task AlterarSenhaInicialAsync(int idPaciente, string novaSenha);
}
