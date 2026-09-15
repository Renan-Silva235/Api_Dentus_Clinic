using DentusClinic.API.DTOs.Request;
using DentusClinic.API.Models;
using DentusClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentusClinic.API.Controllers;

/// <summary>
/// Endpoints do aplicativo mobile do paciente. RN13: o IdPaciente usado para
/// buscar/alterar dados vem sempre das claims do token JWT, nunca de um
/// parâmetro enviado pelo cliente.
/// </summary>
[ApiController]
[Route("api/paciente-app")]
[Authorize(Roles = "PACIENTE")]
public class PacienteAppController : ControllerBase
{
    private readonly IPacienteAppService _pacienteAppService;

    public PacienteAppController(IPacienteAppService pacienteAppService)
    {
        _pacienteAppService = pacienteAppService;
    }

    private int ObterIdPacienteAutenticado()
        => int.Parse(User.FindFirst("Id")!.Value);

    [HttpGet("consultas-futuras")]
    public async Task<IActionResult> ConsultasFuturas()
    {
        var consultas = await _pacienteAppService.ListarConsultasFuturasAsync(ObterIdPacienteAutenticado());
        return Ok(ApiResponse<object>.Ok(consultas));
    }

    [HttpGet("historico")]
    public async Task<IActionResult> Historico()
    {
        var consultas = await _pacienteAppService.ListarHistoricoAsync(ObterIdPacienteAutenticado());
        return Ok(ApiResponse<object>.Ok(consultas));
    }

    [HttpPost("consultas/{id}/confirmar")]
    public async Task<IActionResult> ConfirmarPresenca(int id)
    {
        try
        {
            await _pacienteAppService.ConfirmarPresencaAsync(ObterIdPacienteAutenticado(), id);
            return Ok(ApiResponse<object>.Ok("Presença confirmada com sucesso."));
        }
        catch (InvalidOperationException ex) when (ex.Message == "Consulta não encontrada.")
        {
            return NotFound(ApiResponse<object>.Erro(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Erro(ex.Message));
        }
    }

    [HttpPost("consultas/{id}/recusar")]
    public async Task<IActionResult> RecusarPresenca(int id)
    {
        try
        {
            await _pacienteAppService.RecusarPresencaAsync(ObterIdPacienteAutenticado(), id);
            return Ok(ApiResponse<object>.Ok("Presença recusada; consulta cancelada."));
        }
        catch (InvalidOperationException ex) when (ex.Message == "Consulta não encontrada.")
        {
            return NotFound(ApiResponse<object>.Erro(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Erro(ex.Message));
        }
    }

    [HttpPost("alterar-senha-inicial")]
    public async Task<IActionResult> AlterarSenhaInicial([FromBody] AlterarSenhaInicialRequest request)
    {
        try
        {
            await _pacienteAppService.AlterarSenhaInicialAsync(ObterIdPacienteAutenticado(), request.NovaSenha);
            return Ok(ApiResponse<object>.Ok("Senha definida com sucesso."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Erro(ex.Message));
        }
    }
}
