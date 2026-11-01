using DentusClinic.API.Enums;
using DentusClinic.API.Models;

namespace DentusClinic.API.Services.Interfaces;

public interface INotificadorPush
{
    Task<ResultadoEnvioPushEnum> EnviarAsync(Dispositivo dispositivo, string titulo, string mensagem, CancellationToken cancellationToken);
}
