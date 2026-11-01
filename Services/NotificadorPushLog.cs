using DentusClinic.API.Enums;
using DentusClinic.API.Models;
using DentusClinic.API.Services.Interfaces;

namespace DentusClinic.API.Services;

/// Implementação provisória: registra a notificação no log em vez de entregar.
/// Trocar por um envio real (Firebase) quando o app mobile existir — basta
/// criar outra classe com esta interface e registrar no Program.cs.
public class NotificadorPushLog : INotificadorPush
{
    private readonly ILogger<NotificadorPushLog> _registrador;

    public NotificadorPushLog(ILogger<NotificadorPushLog> registrador)
    {
        _registrador = registrador;
    }

    public Task<ResultadoEnvioPushEnum> EnviarAsync(Dispositivo dispositivo, string titulo, string mensagem, CancellationToken cancellationToken)
    {
        _registrador.LogInformation(
            "PUSH para dispositivo {IdDispositivo} ({Plataforma}) | {Titulo}: {Mensagem}",
            dispositivo.Id, dispositivo.Plataforma, titulo, mensagem);

        return Task.FromResult(ResultadoEnvioPushEnum.Entregue);
    }
}
