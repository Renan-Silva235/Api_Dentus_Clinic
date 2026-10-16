using DentusClinic.API.DTOs.Request;
using DentusClinic.API.Services.Interfaces;
using DentusClinic.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentusClinic.API.Controllers;

[ApiController]
[Route("api/planos")]
[Authorize]
public class PlanosController : ControllerBase
{
    private readonly IPlanosService _planosService;

    public PlanosController(IPlanosService planosService)
    {
        _planosService = planosService;
    }

    /// O claim "Id" guarda o id da entidade autenticada; ele só é um dentista
    /// quando o perfil é DENTISTA. Recepcionista e administrador registram
    /// plano sem dentista vinculado.
    private int? ObterIdDentistaAutenticado()
    {
        if (!User.IsInRole("DENTISTA")) return null;

        return int.TryParse(User.FindFirst("Id")?.Value, out var id) ? id : null;
    }

    [HttpGet]
    public async Task<IActionResult> ListarTodos()
    {
        var planos = await _planosService.ListarTodosAsync();
        return Ok(ApiResponse<object>.Ok(planos));
    }

    [HttpGet("prontuario/{idProntuario}")]
    public async Task<IActionResult> ListarPorProntuario(int idProntuario)
    {
        var planos = await _planosService.ListarPorProntuarioAsync(idProntuario);
        return Ok(ApiResponse<object>.Ok(planos));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var plano = await _planosService.BuscarPorIdAsync(id);
        if (plano is null)
            return NotFound(ApiResponse<object>.Erro("Plano não encontrado."));

        return Ok(ApiResponse<object>.Ok(plano));
    }

    [HttpGet("{id}/historico")]
    public async Task<IActionResult> ListarHistorico(int id)
    {
        var historico = await _planosService.ListarHistoricoAsync(id);
        if (!historico.Any())
            return NotFound(ApiResponse<object>.Erro("Plano não encontrado."));

        return Ok(ApiResponse<object>.Ok(historico));
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar([FromBody] PlanosRequest request)
    {
        var plano = await _planosService.CadastrarAsync(request, ObterIdDentistaAutenticado());
        return CreatedAtAction(nameof(BuscarPorId), new { id = plano.Id },
            ApiResponse<object>.Ok(plano, "Plano cadastrado com sucesso."));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Editar(int id, [FromBody] PlanosRequest request)
    {
        try
        {
            var plano = await _planosService.EditarAsync(id, request, ObterIdDentistaAutenticado());
            if (plano is null)
                return NotFound(ApiResponse<object>.Erro("Plano não encontrado."));

            return Ok(ApiResponse<object>.Ok(plano, "Nova versão do plano registrada com sucesso."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Erro(ex.Message));
        }
    }
}
