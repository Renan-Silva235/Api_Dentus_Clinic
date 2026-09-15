using DentusClinic.API.DTOs.Response;
using DentusClinic.API.Models;
using DentusClinic.API.Repositories.Interfaces;
using DentusClinic.API.Services.Interfaces;

namespace DentusClinic.API.Services;

public class PacienteAppService : IPacienteAppService
{
    private readonly IConsultaService _consultaService;
    private readonly IConsultaRepository _consultaRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly ILoginRepository _loginRepository;

    public PacienteAppService(
        IConsultaService consultaService,
        IConsultaRepository consultaRepository,
        IPacienteRepository pacienteRepository,
        ILoginRepository loginRepository)
    {
        _consultaService = consultaService;
        _consultaRepository = consultaRepository;
        _pacienteRepository = pacienteRepository;
        _loginRepository = loginRepository;
    }

    public Task<IEnumerable<ConsultaResponse>> ListarConsultasFuturasAsync(int idPaciente)
        => _consultaService.ListarFuturasPorPacienteAsync(idPaciente);

    public Task<IEnumerable<ConsultaResponse>> ListarHistoricoAsync(int idPaciente)
        => _consultaService.ListarHistoricoPorPacienteAsync(idPaciente);

    public async Task ConfirmarPresencaAsync(int idPaciente, int idConsulta)
    {
        await ValidarConsultaDoPacienteAsync(idPaciente, idConsulta);
        await _consultaService.ConfirmarPresencaAsync(idConsulta);
    }

    public async Task RecusarPresencaAsync(int idPaciente, int idConsulta)
    {
        await ValidarConsultaDoPacienteAsync(idPaciente, idConsulta);
        await _consultaService.RecusarPresencaAsync(idConsulta);
    }

    public async Task AlterarSenhaInicialAsync(int idPaciente, string novaSenha)
    {
        var paciente = await _pacienteRepository.BuscarPorIdAsync(idPaciente)
            ?? throw new InvalidOperationException("Paciente não encontrado.");

        if (paciente.IdAcesso is null)
            throw new InvalidOperationException("Paciente não possui login vinculado.");

        var login = await _loginRepository.BuscarPorIdAsync(paciente.IdAcesso.Value)
            ?? throw new InvalidOperationException("Login não encontrado.");

        login.Senha = BCrypt.Net.BCrypt.HashPassword(novaSenha);
        login.PrimeiroAcesso = false;
        await _loginRepository.AtualizarAsync(login);
    }

    // RN13: garante que o paciente autenticado só age sobre as próprias consultas.
    // Consulta inexistente ou de outro paciente são tratadas igual (404 no controller),
    // para não revelar a um paciente que a consulta de outro paciente existe.
    private async Task<Consulta> ValidarConsultaDoPacienteAsync(int idPaciente, int idConsulta)
    {
        var consulta = await _consultaRepository.BuscarPorIdAsync(idConsulta)
            ?? throw new InvalidOperationException("Consulta não encontrada.");

        if (consulta.IdPaciente != idPaciente)
            throw new InvalidOperationException("Consulta não encontrada.");

        return consulta;
    }
}
