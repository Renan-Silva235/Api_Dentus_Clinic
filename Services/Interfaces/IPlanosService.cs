using DentusClinic.API.DTOs.Request;
using DentusClinic.API.DTOs.Response;

namespace DentusClinic.API.Services.Interfaces;

public interface IPlanosService
{
    Task<IEnumerable<PlanosResponse>> ListarTodosAsync();
    Task<IEnumerable<PlanosResponse>> ListarPorProntuarioAsync(int idProntuario);
    Task<PlanosResponse?> BuscarPorIdAsync(int id);
    Task<IEnumerable<PlanosResponse>> ListarHistoricoAsync(int id);
    Task<PlanosResponse> CadastrarAsync(PlanosRequest request);
    Task<PlanosResponse?> EditarAsync(int id, PlanosRequest request);
}
