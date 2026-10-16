using DentusClinic.API.DTOs.Request;
using DentusClinic.API.DTOs.Response;
using DentusClinic.API.Interfaces;
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
    private readonly IDispositivoRepository _dispositivoRepository;
    private readonly IProntuarioService _prontuarioService;
    private readonly IPlanosService _planosService;
    private readonly IPacienteService _pacienteService;

    public PacienteAppService(
        IConsultaService consultaService,
        IConsultaRepository consultaRepository,
        IPacienteRepository pacienteRepository,
        ILoginRepository loginRepository,
        IDispositivoRepository dispositivoRepository,
        IProntuarioService prontuarioService,
        IPlanosService planosService,
        IPacienteService pacienteService)
    {
        _pacienteService = pacienteService;
        _consultaService = consultaService;
        _consultaRepository = consultaRepository;
        _pacienteRepository = pacienteRepository;
        _loginRepository = loginRepository;
        _dispositivoRepository = dispositivoRepository;
        _prontuarioService = prontuarioService;
        _planosService = planosService;
    }

    public async Task<PacienteResponse?> BuscarPerfilAsync(int idPaciente)
    {
        await GarantirSenhaDefinidaAsync(idPaciente);
        return await _pacienteService.BuscarPorIdAsync(idPaciente);
    }

    /// O paciente edita os próprios dados. O id vem sempre do token, então não
    /// há como alterar o cadastro de outra pessoa.
    public async Task<PacienteResponse?> AtualizarPerfilAsync(int idPaciente, PacienteEditarRequest request)
    {
        await GarantirSenhaDefinidaAsync(idPaciente);
        return await _pacienteService.EditarAsync(idPaciente, request);
    }

    /// Troca de senha com o paciente já logado, diferente da troca obrigatória
    /// do primeiro acesso: aqui a senha atual precisa ser conferida.
    public async Task AlterarSenhaAsync(int idPaciente, AlterarSenhaRequest request)
    {
        var paciente = await _pacienteRepository.BuscarPorIdAsync(idPaciente)
            ?? throw new InvalidOperationException("Paciente não encontrado.");

        if (paciente.IdAcesso is null)
            throw new InvalidOperationException("Paciente não possui login vinculado.");

        var login = await _loginRepository.BuscarPorIdAsync(paciente.IdAcesso.Value)
            ?? throw new InvalidOperationException("Login não encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(request.SenhaAtual, login.Senha))
            throw new InvalidOperationException("Senha atual incorreta.");

        login.Senha = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
        login.PrimeiroAcesso = false;
        await _loginRepository.AtualizarAsync(login);
    }

    /// Os tratamentos do paciente são os planos do prontuário dele. A listagem
    /// já devolve só a versão atual de cada plano, nunca o histórico.
    public async Task<IEnumerable<PlanosResponse>> ListarTratamentosAsync(int idPaciente)
    {
        await GarantirSenhaDefinidaAsync(idPaciente);

        var prontuario = await _prontuarioService.BuscarPorPacienteAsync(idPaciente);
        if (prontuario is null) return [];

        return await _planosService.ListarPorProntuarioAsync(prontuario.Id);
    }

    // O token é do aparelho, não do paciente: ao reinstalar o aplicativo ou
    // trocar de usuário, o mesmo token pode chegar vinculado a outra pessoa.
    // Por isso a gravação é por token, e não um insere novo a cada chamada.
    public async Task RegistrarDispositivoAsync(int idPaciente, DispositivoRequest request)
    {
        await GarantirSenhaDefinidaAsync(idPaciente);

        var existente = await _dispositivoRepository.BuscarPorTokenAsync(request.TokenPush);

        if (existente is not null)
        {
            existente.IdPaciente = idPaciente;
            existente.Plataforma = request.Plataforma;
            existente.DataCadastro = DateTime.UtcNow;
            existente.Ativo = true;
            await _dispositivoRepository.AtualizarAsync(existente);
            return;
        }

        await _dispositivoRepository.AdicionarAsync(new Dispositivo
        {
            IdPaciente = idPaciente,
            TokenPush = request.TokenPush,
            Plataforma = request.Plataforma,
            Ativo = true
        });
    }

    public async Task<IEnumerable<ConsultaResponse>> ListarConsultasFuturasAsync(int idPaciente)
    {
        await GarantirSenhaDefinidaAsync(idPaciente);
        return await _consultaService.ListarFuturasPorPacienteAsync(idPaciente);
    }

    public async Task<IEnumerable<ConsultaResponse>> ListarHistoricoAsync(int idPaciente)
    {
        await GarantirSenhaDefinidaAsync(idPaciente);
        return await _consultaService.ListarHistoricoPorPacienteAsync(idPaciente);
    }

    public async Task ConfirmarPresencaAsync(int idPaciente, int idConsulta)
    {
        await GarantirSenhaDefinidaAsync(idPaciente);
        await ValidarConsultaDoPacienteAsync(idPaciente, idConsulta);
        await _consultaService.ConfirmarPresencaAsync(idConsulta);
    }

    public async Task RecusarPresencaAsync(int idPaciente, int idConsulta)
    {
        await GarantirSenhaDefinidaAsync(idPaciente);
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

    // Enquanto a senha temporária não for trocada, o único endpoint liberado é
    // o de troca de senha. A verificação é feita no banco, e não por claim do
    // token, para a liberação valer no mesmo instante em que a senha é trocada.
    private async Task GarantirSenhaDefinidaAsync(int idPaciente)
    {
        var paciente = await _pacienteRepository.BuscarPorIdAsync(idPaciente)
            ?? throw new InvalidOperationException("Paciente não encontrado.");

        if (paciente.IdAcesso is null) return;

        var login = await _loginRepository.BuscarPorIdAsync(paciente.IdAcesso.Value);
        if (login is not null && login.PrimeiroAcesso)
            throw new InvalidOperationException(
                "Defina uma nova senha antes de usar o aplicativo.");
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
