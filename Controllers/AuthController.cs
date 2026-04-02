using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VisaFlow.DTOs;
using VisaFlow.Services;

namespace VisaFlow.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        /// <summary>
        /// Login. Retorna token JWT válido por 8 horas.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var (sucesso, mensagem, response) = await _service.LoginAsync(dto);
            if (!sucesso) return Unauthorized(new { mensagem });
            return Ok(response);
        }

        /// <summary>
        /// Altera a password do utilizador autenticado.
        /// </summary>
        [HttpPost("alterar-password")]
        [Authorize]
        public async Task<IActionResult> AlterarPassword([FromBody] AlterarPasswordDto dto)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var sucesso = await _service.AlterarPasswordAsync(usuarioId, dto.PasswordAntiga, dto.PasswordNova);
            if (!sucesso) return BadRequest(new { mensagem = "Password antiga incorrecta." });
            return Ok(new { mensagem = "Password alterada com sucesso." });
        }

        /// <summary>
        /// Retorna dados do utilizador autenticado.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                nome = User.FindFirst(ClaimTypes.Name)?.Value,
                email = User.FindFirst(ClaimTypes.Email)?.Value,
                role = User.FindFirst(ClaimTypes.Role)?.Value
            });
        }
    }

    public class AlterarPasswordDto
    {
        public string PasswordAntiga { get; set; } = string.Empty;
        public string PasswordNova { get; set; } = string.Empty;
    }
}
