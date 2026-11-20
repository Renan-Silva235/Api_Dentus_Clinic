using DentusClinic.API.DTOs.Request;
using DentusClinic.API.Services.Interfaces;
using DentusClinic.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DentusClinic.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var resultado = await _authService.LoginAsync(request);
        if (resultado is null)
            return Unauthorized(ApiResponse<object>.Erro("E-mail ou senha inválidos."));

        return Ok(ApiResponse<object>.Ok(resultado, "Login realizado com sucesso."));
    }

    // Primeiro passo do "esqueci minha senha": só confirma que o e-mail existe,
    // antes do app pedir a nova senha.
    [HttpPost("esqueci-senha/verificar")]
    public async Task<IActionResult> VerificarEmailEsqueciSenha([FromBody] EsqueciSenhaVerificarRequest request)
    {
        if (!await _authService.EmailExisteAsync(request.Email))
            return NotFound(ApiResponse<object>.Erro("E-mail não encontrado."));

        return Ok(ApiResponse<object>.Ok("E-mail encontrado."));
    }

    // Segundo passo: troca a senha direto a partir do e-mail, sem confirmação
    // adicional (sem envio de e-mail/token nesse fluxo).
    [HttpPost("esqueci-senha/redefinir")]
    public async Task<IActionResult> RedefinirSenhaEsquecida([FromBody] EsqueciSenhaRedefinirRequest request)
    {
        try
        {
            await _authService.RedefinirSenhaAsync(request.Email, request.NovaSenha);
            return Ok(ApiResponse<object>.Ok("Senha redefinida com sucesso."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Erro(ex.Message));
        }
    }
}
