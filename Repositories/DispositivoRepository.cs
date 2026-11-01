using DentusClinic.API.Data;
using DentusClinic.API.Models;
using DentusClinic.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentusClinic.API.Repositories;

public class DispositivoRepository : IDispositivoRepository
{
    private readonly AppDbContext _contexto;

    public DispositivoRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IEnumerable<Dispositivo>> ListarAtivosPorPacienteAsync(int idPaciente)
        => await _contexto.Dispositivos
            .Where(d => d.IdPaciente == idPaciente && d.Ativo)
            .ToListAsync();

    public async Task<Dispositivo?> BuscarPorTokenAsync(string tokenPush)
        => await _contexto.Dispositivos.FirstOrDefaultAsync(d => d.TokenPush == tokenPush);

    public async Task AdicionarAsync(Dispositivo dispositivo)
    {
        _contexto.Dispositivos.Add(dispositivo);
        await _contexto.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Dispositivo dispositivo)
    {
        _contexto.Dispositivos.Update(dispositivo);
        await _contexto.SaveChangesAsync();
    }
}
