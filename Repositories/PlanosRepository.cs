using DentusClinic.API.Data;
using DentusClinic.API.Models;
using DentusClinic.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentusClinic.API.Repositories;

public class PlanosRepository : IPlanosRepository
{
    private readonly AppDbContext _contexto;

    public PlanosRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IEnumerable<Planos>> ListarTodosAsync()
        => await _contexto.Planos
            .Where(p => p.VersaoAtual)
            .Include(p => p.Servico).Include(p => p.Dentista)
            .ToListAsync();

    public async Task<IEnumerable<Planos>> ListarPorProntuarioAsync(int idProntuario)
        => await _contexto.Planos
            .Where(p => p.IdProntuario == idProntuario && p.VersaoAtual)
            .Include(p => p.Servico).Include(p => p.Dentista)
            .ToListAsync();

    public async Task<Planos?> BuscarPorIdAsync(int id)
        => await _contexto.Planos.Include(p => p.Servico).Include(p => p.Dentista).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Planos>> ListarCadeiaAsync(int id)
    {
        var plano = await _contexto.Planos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (plano is null) return [];

        var doProntuario = await _contexto.Planos
            .AsNoTracking()
            .Where(p => p.IdProntuario == plano.IdProntuario)
            .Include(p => p.Servico).Include(p => p.Dentista)
            .ToListAsync();

        var porId = doProntuario.ToDictionary(p => p.Id);
        var proximaVersao = doProntuario
            .Where(p => p.PlanoAnteriorId is not null)
            .ToLookup(p => p.PlanoAnteriorId!.Value);

        // percorre a cadeia nas duas direções para o histórico sair igual
        // independente de qual versão do plano foi informada
        var cadeia = new List<Planos>();
        var visitados = new HashSet<int>();

        var anterior = porId[id];
        while (anterior is not null && visitados.Add(anterior.Id))
        {
            cadeia.Add(anterior);
            anterior = anterior.PlanoAnteriorId is int idAnterior ? porId.GetValueOrDefault(idAnterior) : null;
        }

        var posterior = proximaVersao[id].FirstOrDefault();
        while (posterior is not null && visitados.Add(posterior.Id))
        {
            cadeia.Add(posterior);
            posterior = proximaVersao[posterior.Id].FirstOrDefault();
        }

        return cadeia.OrderBy(p => p.DataAtualizacao).ThenBy(p => p.Id);
    }

    public async Task AdicionarAsync(Planos plano)
    {
        _contexto.Planos.Add(plano);
        await _contexto.SaveChangesAsync();
    }

    public async Task RegistrarNovaVersaoAsync(Planos planoAtual, Planos novaVersao)
    {
        await using var transacao = await _contexto.Database.BeginTransactionAsync();

        planoAtual.VersaoAtual = false;
        _contexto.Planos.Add(novaVersao);
        await _contexto.SaveChangesAsync();

        await transacao.CommitAsync();
    }
}
