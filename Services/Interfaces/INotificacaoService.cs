namespace DentusClinic.API.Services.Interfaces;

public interface INotificacaoService
{
    /// Notifica as consultas que começam nas próximas 24h e ainda estão sem
    /// resposta do paciente. Devolve quantas notificações foram entregues.
    Task<int> ProcessarLembretesAsync(CancellationToken cancellationToken);
}
