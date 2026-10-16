using DentusClinic.API.Enums;
using DentusClinic.API.Models;
using DentusClinic.API.Repositories.Interfaces;
using DentusClinic.API.Services.Interfaces;

namespace DentusClinic.API.Services;

public class NotificacaoService : INotificacaoService
{
    private const string TipoConfirmacao = "Confirmação de consulta";

    private static readonly TimeSpan AntecedenciaMaxima = TimeSpan.FromHours(24);

    // O paciente pode responder até o horário da consulta, então o lembrete
    // vale até lá. Depois disso não há mais o que confirmar.
    private static readonly TimeSpan AntecedenciaMinima = TimeSpan.Zero;

    private static readonly TimeSpan IntervaloEntreLembretes = TimeSpan.FromHours(1);

    private readonly IConsultaRepository _consultaRepository;
    private readonly INotificacaoRepository _notificacaoRepository;
    private readonly IDispositivoRepository _dispositivoRepository;
    private readonly INotificadorPush _notificadorPush;
    private readonly ILogger<NotificacaoService> _registrador;

    public NotificacaoService(
        IConsultaRepository consultaRepository,
        INotificacaoRepository notificacaoRepository,
        IDispositivoRepository dispositivoRepository,
        INotificadorPush notificadorPush,
        ILogger<NotificacaoService> registrador)
    {
        _consultaRepository = consultaRepository;
        _notificacaoRepository = notificacaoRepository;
        _dispositivoRepository = dispositivoRepository;
        _notificadorPush = notificadorPush;
        _registrador = registrador;
    }

    public async Task<int> ProcessarLembretesAsync(CancellationToken cancellationToken)
    {
        var agora = DateTime.Now;
        var janelaInicio = agora + AntecedenciaMinima;
        var janelaFim = agora + AntecedenciaMaxima;

        // O filtro em SQL é por data porque DataConsulta e HoraConsulta são
        // colunas separadas; o recorte exato por horário acontece abaixo.
        var candidatas = await _consultaRepository.ListarPendentesConfirmacaoAsync(
            DateOnly.FromDateTime(janelaInicio),
            DateOnly.FromDateTime(janelaFim));

        var entregues = 0;

        foreach (var consulta in candidatas)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var horario = consulta.DataConsulta.ToDateTime(consulta.HoraConsulta);
            if (horario < janelaInicio || horario > janelaFim) continue;

            var notificacao = await _notificacaoRepository.BuscarPorConsultaAsync(consulta.Id);

            if (notificacao?.DataEnvio is DateTime ultimoEnvio && agora - ultimoEnvio < IntervaloEntreLembretes)
                continue;

            if (notificacao is null)
            {
                notificacao = new Notificacao
                {
                    IdConsulta = consulta.Id,
                    TipoNotificacao = TipoConfirmacao
                };
                await _notificacaoRepository.AdicionarAsync(notificacao);
            }

            var entregue = await EnviarAoPacienteAsync(consulta, cancellationToken);

            notificacao.Tentativas++;
            notificacao.DataEnvio = agora;
            notificacao.StatusEnvio = entregue ? StatusEnvioEnum.Enviada : StatusEnvioEnum.Falhou;
            await _notificacaoRepository.AtualizarAsync(notificacao);

            if (entregue) entregues++;
        }

        return entregues;
    }

    private async Task<bool> EnviarAoPacienteAsync(Consulta consulta, CancellationToken cancellationToken)
    {
        var dispositivos = await _dispositivoRepository.ListarAtivosPorPacienteAsync(consulta.IdPaciente);
        if (!dispositivos.Any())
        {
            _registrador.LogWarning(
                "Consulta {IdConsulta} não notificada: paciente {IdPaciente} não tem dispositivo ativo.",
                consulta.Id, consulta.IdPaciente);
            return false;
        }

        var data = consulta.DataConsulta.ToString("dd/MM/yyyy");
        var hora = consulta.HoraConsulta.ToString("HH:mm");
        var mensagem = $"Você tem consulta em {data} às {hora}. Confirme sua presença.";

        var entregue = false;
        foreach (var dispositivo in dispositivos)
        {
            var resultado = await _notificadorPush.EnviarAsync(dispositivo, TipoConfirmacao, mensagem, cancellationToken);

            if (resultado == ResultadoEnvioPushEnum.Entregue)
            {
                entregue = true;
                continue;
            }

            // Token sem conserto: desativar evita tentar entregar nele a cada
            // hora, para sempre. O paciente volta a ter um ao abrir o aplicativo.
            if (resultado == ResultadoEnvioPushEnum.TokenInvalido)
            {
                dispositivo.Ativo = false;
                await _dispositivoRepository.AtualizarAsync(dispositivo);

                _registrador.LogInformation(
                    "Dispositivo {IdDispositivo} desativado: o Firebase não aceita mais o token dele.",
                    dispositivo.Id);
            }
        }

        return entregue;
    }
}
