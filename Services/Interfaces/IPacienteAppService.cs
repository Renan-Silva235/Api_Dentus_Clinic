using DentusClinic.API.DTOs.Response;

namespace DentusClinic.API.Services.Interfaces;

public interface IPacienteAppService
{
    Task<IEnumerable<ConsultaResponse>> ListarConsultasFuturasAsync(int idPaciente);
    Task<IEnumerable<ConsultaResponse>> ListarHistoricoAsync(int idPaciente);
    Task ConfirmarPresencaAsync(int idPaciente, int idConsulta);
    Task RecusarPresencaAsync(int idPaciente, int idConsulta);
    Task AlterarSenhaInicialAsync(int idPaciente, string novaSenha);
}
