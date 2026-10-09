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
}
