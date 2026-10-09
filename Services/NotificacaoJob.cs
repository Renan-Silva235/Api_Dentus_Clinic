using DentusClinic.API.Services.Interfaces;

namespace DentusClinic.API.Services;

/// Roda ao subir a API e depois de hora em hora: é o reenvio de hora em hora
/// enquanto o paciente não responde que exige essa frequência.
public class NotificacaoJob : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _fabricaDeEscopo;
    private readonly ILogger<NotificacaoJob> _registrador;

    public NotificacaoJob(IServiceScopeFactory fabricaDeEscopo, ILogger<NotificacaoJob> registrador)
    {
        _fabricaDeEscopo = fabricaDeEscopo;
        _registrador = registrador;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var temporizador = new PeriodicTimer(Intervalo);

        try
        {
            do
            {
                await ProcessarAsync(stoppingToken);
            }
            while (await temporizador.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // API encerrando
        }
    }

    private async Task ProcessarAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var escopo = _fabricaDeEscopo.CreateScope();
            var servico = escopo.ServiceProvider.GetRequiredService<INotificacaoService>();

            var entregues = await servico.ProcessarLembretesAsync(stoppingToken);
            _registrador.LogInformation("Lembretes de consulta: {Entregues} notificação(ões) entregue(s).", entregues);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception excecao)
        {
            // Uma falha não pode derrubar o job: na próxima hora ele tenta de novo.
            _registrador.LogError(excecao, "Falha ao processar os lembretes de consulta.");
        }
    }
}
