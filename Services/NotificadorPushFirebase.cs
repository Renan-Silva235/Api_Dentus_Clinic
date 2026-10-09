using DentusClinic.API.Models;
using DentusClinic.API.Services.Interfaces;
using FirebaseAdmin.Messaging;

namespace DentusClinic.API.Services;

public class NotificadorPushFirebase : INotificadorPush
{
    private readonly ILogger<NotificadorPushFirebase> _registrador;

    public NotificadorPushFirebase(ILogger<NotificadorPushFirebase> registrador)
    {
        _registrador = registrador;
    }

    public async Task<bool> EnviarAsync(Dispositivo dispositivo, string titulo, string mensagem, CancellationToken cancellationToken)
    {
        var recado = new Message
        {
            Token = dispositivo.TokenPush,
            Notification = new Notification { Title = titulo, Body = mensagem }
        };

        try
        {
            var id = await FirebaseMessaging.DefaultInstance.SendAsync(recado, cancellationToken);
            _registrador.LogInformation(
                "Push entregue ao Firebase para o dispositivo {IdDispositivo}: {IdMensagem}",
                dispositivo.Id, id);
            return true;
        }
        catch (FirebaseMessagingException excecao)
        {
            // Token inválido ou expirado é o caso comum: o app foi desinstalado
            // ou o token mudou. Não é erro de código, então não derruba o job.
            _registrador.LogWarning(excecao,
                "Firebase recusou o push para o dispositivo {IdDispositivo}: {Motivo}",
                dispositivo.Id, excecao.MessagingErrorCode);
            return false;
        }
    }
}
