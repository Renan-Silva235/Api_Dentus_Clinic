using DentusClinic.API.Data;
using DentusClinic.API.Models;
using DentusClinic.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentusClinic.API.Repositories;

public class NotificacaoRepository : INotificacaoRepository
{
    private readonly AppDbContext _contexto;

    public NotificacaoRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Notificacao?> BuscarPorConsultaAsync(int idConsulta)
        => await _contexto.Notificacoes.FirstOrDefaultAsync(n => n.IdConsulta == idConsulta);

    public async Task AdicionarAsync(Notificacao notificacao)
    {
        _contexto.Notificacoes.Add(notificacao);
        await _contexto.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Notificacao notificacao)
    {
        _contexto.Notificacoes.Update(notificacao);
        await _contexto.SaveChangesAsync();
    }
}
