using DentusClinic.API.Models;

namespace DentusClinic.API.Repositories.Interfaces;

public interface INotificacaoRepository
{
    Task<Notificacao?> BuscarPorConsultaAsync(int idConsulta);
    Task AdicionarAsync(Notificacao notificacao);
    Task AtualizarAsync(Notificacao notificacao);
}
