using DentusClinic.API.DTOs.Request;
using DentusClinic.API.DTOs.Response;
using DentusClinic.API.Models;
using DentusClinic.API.Repositories.Interfaces;
using DentusClinic.API.Services.Interfaces;

namespace DentusClinic.API.Services;

public class PlanosService : IPlanosService
{
    private readonly IPlanosRepository _planosRepository;
    private readonly IProntuarioRepository _prontuarioRepository;

    public PlanosService(IPlanosRepository planosRepository, IProntuarioRepository prontuarioRepository)
    {
        _planosRepository = planosRepository;
        _prontuarioRepository = prontuarioRepository;
    }

    public async Task<IEnumerable<PlanosResponse>> ListarTodosAsync()
    {
        var lista = await _planosRepository.ListarTodosAsync();
        return lista.Select(MapearResponse);
    }

    public async Task<IEnumerable<PlanosResponse>> ListarPorProntuarioAsync(int idProntuario)
    {
        var lista = await _planosRepository.ListarPorProntuarioAsync(idProntuario);
        return lista.Select(MapearResponse);
    }

    public async Task<PlanosResponse?> BuscarPorIdAsync(int id)
    {
        var plano = await _planosRepository.BuscarPorIdAsync(id);
        return plano is null ? null : MapearResponse(plano);
    }

    public async Task<IEnumerable<PlanosResponse>> ListarHistoricoAsync(int id)
    {
        var cadeia = await _planosRepository.ListarCadeiaAsync(id);
        return cadeia.Select(MapearResponse).ToList();
    }

    public async Task<PlanosResponse> CadastrarAsync(PlanosRequest request)
    {
        var plano = new Planos
        {
            IdProntuario = request.IdProntuario,
            IdServico = request.IdServico,
            Descricao = request.Descricao,
            Condicao = request.Condicao,
            Status = request.Status,
            Observacao = request.Observacao,
            Dente = request.Dente,
            VersaoAtual = true,
            DataAtualizacao = DateOnly.FromDateTime(DateTime.Today)
        };

        await _planosRepository.AdicionarAsync(plano);

        var planoSalvo = await _planosRepository.BuscarPorIdAsync(plano.Id);
        return MapearResponse(planoSalvo!);
    }

    // Planos são append-only: editar significa registrar uma nova versão,
    // mantendo o registro anterior intacto para fins legais e de auditoria.
    public async Task<PlanosResponse?> EditarAsync(int id, PlanosRequest request)
    {
        var planoAtual = await _planosRepository.BuscarPorIdAsync(id);
        if (planoAtual is null) return null;

        if (!planoAtual.VersaoAtual)
            throw new InvalidOperationException(
                "Este plano não é a versão atual. Apenas a versão atual pode receber uma nova versão.");

        var novaVersao = new Planos
        {
            IdProntuario = planoAtual.IdProntuario,
            IdServico = request.IdServico,
            Descricao = request.Descricao,
            Condicao = request.Condicao,
            Status = request.Status,
            Observacao = request.Observacao,
            Dente = request.Dente,
            PlanoAnteriorId = planoAtual.Id,
            VersaoAtual = true,
            DataAtualizacao = DateOnly.FromDateTime(DateTime.Today)
        };

        await _planosRepository.RegistrarNovaVersaoAsync(planoAtual, novaVersao);
        await _prontuarioRepository.AtualizarDataAsync(novaVersao.IdProntuario, novaVersao.DataAtualizacao.Value);

        var planoSalvo = await _planosRepository.BuscarPorIdAsync(novaVersao.Id);
        return MapearResponse(planoSalvo!);
    }

    private static PlanosResponse MapearResponse(Planos p) => new()
    {
        Id = p.Id,
        IdProntuario = p.IdProntuario,
        IdServico = p.IdServico,
        NomeServico = p.Servico?.Nome ?? string.Empty,
        Descricao = p.Descricao,
        Condicao = p.Condicao,
        Status = p.Status,
        Observacao = p.Observacao,
        Dente = p.Dente,
        DataCriacao = p.DataCriacao,
        DataAtualizacao = p.DataAtualizacao,
        PlanoAnteriorId = p.PlanoAnteriorId,
        VersaoAtual = p.VersaoAtual
    };
}
