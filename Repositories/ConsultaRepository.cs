using DentusClinic.API.Data;
using DentusClinic.API.Enums;
using DentusClinic.API.Models;
using DentusClinic.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentusClinic.API.Repositories;

public class ConsultaRepository : IConsultaRepository
{
    private readonly AppDbContext _contexto;

    public ConsultaRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IEnumerable<Consulta>> ListarTodosAsync()
        => await _contexto.Consultas
            .Where(c => c.Status != "Inativa")
            .Include(c => c.Dentista).Include(c => c.Paciente).Include(c => c.Servico)
            .ToListAsync();

    public async Task<IEnumerable<Consulta>> ListarHojeAsync()
        => await _contexto.Consultas
            .Where(c => c.DataConsulta == DateOnly.FromDateTime(DateTime.Today) && c.Status != "Inativa")
            .Include(c => c.Dentista).Include(c => c.Paciente).Include(c => c.Servico)
            .OrderBy(c => c.HoraConsulta)
            .ToListAsync();

    public async Task<IEnumerable<Consulta>> ListarPendentesConfirmacaoAsync(DateOnly inicio, DateOnly fim)
        => await _contexto.Consultas
            .Where(c => c.DataConsulta >= inicio && c.DataConsulta <= fim
                     && c.StatusConfirmacao == StatusConfirmacaoEnum.Pendente
                     && c.Status != "Cancelada" && c.Status != "Inativa"
                     && c.Status != "Encerrada" && c.Status != "Concluida")
            .OrderBy(c => c.DataConsulta).ThenBy(c => c.HoraConsulta)
            .ToListAsync();

    public async Task<IEnumerable<Consulta>> ListarPorDentistaEDataAsync(int idDentista, DateOnly data)
        => await _contexto.Consultas
            .Where(c => c.IdDentista == idDentista && c.DataConsulta == data
                     && c.Status != "Inativa" && c.Status != "Cancelada")
            .Include(c => c.Dentista).Include(c => c.Paciente).Include(c => c.Servico)
            .OrderBy(c => c.HoraConsulta)
            .ToListAsync();

    public async Task<IEnumerable<Consulta>> ListarFuturasPorPacienteAsync(int idPaciente)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var horaAgora = TimeOnly.FromDateTime(DateTime.Now);

        return await _contexto.Consultas
            .Where(c => c.IdPaciente == idPaciente
                     && c.Status != "Cancelada" && c.Status != "Inativa"
                     && (c.DataConsulta > hoje || (c.DataConsulta == hoje && c.HoraConsulta >= horaAgora)))
            .Include(c => c.Dentista).Include(c => c.Paciente).Include(c => c.Servico)
            .OrderBy(c => c.DataConsulta).ThenBy(c => c.HoraConsulta)
            .ToListAsync();
    }

    public async Task<IEnumerable<Consulta>> ListarHistoricoPorPacienteAsync(int idPaciente)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var horaAgora = TimeOnly.FromDateTime(DateTime.Now);

        return await _contexto.Consultas
            .Where(c => c.IdPaciente == idPaciente
                     && c.Status != "Inativa"
                     && (c.DataConsulta < hoje || (c.DataConsulta == hoje && c.HoraConsulta < horaAgora)))
            .Include(c => c.Dentista).Include(c => c.Paciente).Include(c => c.Servico)
            .OrderByDescending(c => c.DataConsulta).ThenByDescending(c => c.HoraConsulta)
            .ToListAsync();
    }

    public async Task<Consulta?> BuscarPorIdAsync(int id)
        => await _contexto.Consultas.Include(c => c.Dentista).Include(c => c.Paciente).Include(c => c.Servico).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<bool> ExisteConflitoAsync(int idDentista, DateOnly data, TimeOnly hora, int? idExcluido = null)
    {
        var query = _contexto.Consultas
            .Where(c => c.IdDentista == idDentista
                     && c.DataConsulta == data
                     && c.HoraConsulta == hora
                     && c.Status != "Cancelada"
                     && c.Status != "Inativa");

        if (idExcluido.HasValue)
            query = query.Where(c => c.Id != idExcluido.Value);

        return await query.AnyAsync();
    }

    public async Task AdicionarAsync(Consulta consulta)
    {
        _contexto.Consultas.Add(consulta);
        await _contexto.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Consulta consulta)
        => await _contexto.SaveChangesAsync();
}
