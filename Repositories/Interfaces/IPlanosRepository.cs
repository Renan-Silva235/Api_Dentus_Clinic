using DentusClinic.API.Models;

namespace DentusClinic.API.Repositories.Interfaces;

public interface IPlanosRepository
{
    Task<IEnumerable<Planos>> ListarTodosAsync();
    Task<IEnumerable<Planos>> ListarPorProntuarioAsync(int idProntuario);
    Task<Planos?> BuscarPorIdAsync(int id);
    Task<IEnumerable<Planos>> ListarCadeiaAsync(int id);
    Task AdicionarAsync(Planos plano);
    Task RegistrarNovaVersaoAsync(Planos planoAtual, Planos novaVersao);
}
