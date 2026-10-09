using DentusClinic.API.Models;

namespace DentusClinic.API.Services.Interfaces;

public interface INotificadorPush
{
    Task<bool> EnviarAsync(Dispositivo dispositivo, string titulo, string mensagem, CancellationToken cancellationToken);
}
