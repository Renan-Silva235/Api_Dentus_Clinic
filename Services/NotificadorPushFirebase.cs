using DentusClinic.API.Enums;
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

    public async Task<ResultadoEnvioPushEnum> EnviarAsync(Dispositivo dispositivo, string titulo, string mensagem, CancellationToken cancellationToken)
    {
        var recado = new Message
        {
            Token = dispositivo.TokenPush,
            Notification = new Notification { Title = titulo, Body = mensagem },
            // Sem o canal de importância alta e a prioridade, o Android entrega
            // a notificação calada, só na barra, sem a faixa na tela.
            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    ChannelId = "consultas",
                    Priority = NotificationPriority.MAX,
                    DefaultSound = true,
                    DefaultVibrateTimings = true
                }
            }
        };

        try
        {
            var id = await FirebaseMessaging.DefaultInstance.SendAsync(recado, cancellationToken);
            _registrador.LogInformation(
                "Push entregue ao Firebase para o dispositivo {IdDispositivo}: {IdMensagem}",
                dispositivo.Id, id);
            return ResultadoEnvioPushEnum.Entregue;
        }
        catch (FirebaseMessagingException excecao)
        {
            var resultado = Classificar(excecao.MessagingErrorCode);

            _registrador.LogWarning(excecao,
                "Firebase recusou o push para o dispositivo {IdDispositivo}: {Motivo} ({Resultado})",
                dispositivo.Id, excecao.MessagingErrorCode, resultado);

            return resultado;
        }
    }

    /// Separa o que não tem conserto do que é passageiro. Aplicativo
    /// desinstalado, token de outro projeto ou token malformado não voltam a
    /// funcionar; serviço indisponível ou limite excedido, sim.
    private static ResultadoEnvioPushEnum Classificar(MessagingErrorCode? codigo) => codigo switch
    {
        MessagingErrorCode.Unregistered => ResultadoEnvioPushEnum.TokenInvalido,
        MessagingErrorCode.SenderIdMismatch => ResultadoEnvioPushEnum.TokenInvalido,
        MessagingErrorCode.InvalidArgument => ResultadoEnvioPushEnum.TokenInvalido,
        _ => ResultadoEnvioPushEnum.FalhaTemporaria
    };
}
