using DentusClinic.API.Data;
using DentusClinic.API.DTOs.Request;
using DentusClinic.API.DTOs.Response;
using DentusClinic.API.Enums;
using DentusClinic.API.Interfaces;
using DentusClinic.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DentusClinic.API.Services;

public class PacienteService : IPacienteService
{
    private readonly AppDbContext _context;

    public PacienteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PacienteResponse>> ListarTodosAsync()
    {
        var lista = await _context.Pacientes.Where(p => p.Ativo).ToListAsync();
        return lista.Select(p => MapearResponse(p));
    }

    public async Task<PacienteResponse?> BuscarPorIdAsync(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        return paciente is null ? null : MapearResponse(paciente);
    }

    public async Task<PacienteResponse> CadastrarAsync(PacienteRequest request)
    {
        if (await _context.Pacientes.AnyAsync(p => p.Cpf == request.Cpf))
            throw new InvalidOperationException("CPF já cadastrado no sistema.");
        if (await _context.Pacientes.AnyAsync(p => p.Email == request.Email))
            throw new InvalidOperationException("Email já cadastrado no sistema.");
        if (await _context.Logins.AnyAsync(l => l.Email == request.Email))
            throw new InvalidOperationException("Email já cadastrado no sistema.");

        // RN12: Login com senha temporária, criado automaticamente junto do paciente
        // (mesmo padrão do FuncionarioService), para acesso ao app mobile.
        var senhaTemporaria = Random.Shared.Next(100000, 999999).ToString();
        var login = new Login
        {
            Email = request.Email,
            Senha = BCrypt.Net.BCrypt.HashPassword(senhaTemporaria),
            TipoAcesso = TiposAcessoEnum.PACIENTE,
            PrimeiroAcesso = true
        };
        _context.Logins.Add(login);
        await _context.SaveChangesAsync();

        var paciente = new Paciente
        {
            Nome = request.Nome,
            Cpf = request.Cpf,
            Telefone = request.Telefone,
            Email = request.Email,
            DataNascimento = request.DataNascimento,
            Endereco = request.Endereco,
            IdAcesso = login.Id
        };

        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        // Prontuário criado automaticamente ao cadastrar paciente
        var prontuario = new Prontuario
        {
            IdPaciente = paciente.Id,
            DataAbertura = DateOnly.FromDateTime(DateTime.Today)
        };
        _context.Prontuarios.Add(prontuario);
        await _context.SaveChangesAsync();

        // A senha temporária só existe em texto puro neste momento (depois fica só o
        // hash) — vai na resposta do cadastro para quem cadastrou repassar ao paciente.
        return MapearResponse(paciente, senhaTemporaria);
    }

    public async Task<PacienteResponse?> EditarAsync(int id, PacienteEditarRequest request)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente is null) return null;

        if (request.Email != null)
        {
            if (await _context.Pacientes.AnyAsync(p => p.Email == request.Email && p.Id != id))
                throw new InvalidOperationException("E-mail já cadastrado no sistema.");
            paciente.Email = request.Email;
        }

        if (request.Nome != null)
            paciente.Nome = request.Nome;

        if (request.Telefone != null)
            paciente.Telefone = request.Telefone;

        if (request.DataNascimento != null)
            paciente.DataNascimento = request.DataNascimento.Value;

        if (request.Endereco != null)
            paciente.Endereco = request.Endereco;

        await _context.SaveChangesAsync();
        return MapearResponse(paciente);
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente is null || !paciente.Ativo) return false;

        paciente.Ativo = false;
        await _context.SaveChangesAsync();
        return true;
    }

    private static PacienteResponse MapearResponse(Paciente p, string? senhaTemporaria = null) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Cpf = p.Cpf,
        Telefone = p.Telefone,
        Email = p.Email,
        DataNascimento = p.DataNascimento,
        Endereco = p.Endereco,
        SenhaTemporaria = senhaTemporaria
    };
}
