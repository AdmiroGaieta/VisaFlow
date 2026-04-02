using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VisaFlow.Data;
using VisaFlow.DTOs;
using VisaFlow.Models;

namespace VisaFlow.Services
{
    public interface IAuthService
    {
        Task<(bool sucesso, string mensagem, LoginResponseDto? response)> LoginAsync(LoginDto dto);
        Task<bool> AlterarPasswordAsync(int usuarioId, string passwordAntiga, string passwordNova);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<(bool sucesso, string mensagem, LoginResponseDto? response)> LoginAsync(LoginDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Activo);

            if (usuario == null)
                return (false, "Email ou password incorrectos.", null);

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
                return (false, "Email ou password incorrectos.", null);

            usuario.UltimoLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            var token = GerarToken(usuario);
            var expiracao = DateTime.Now.AddHours(8);

            return (true, "Login realizado com sucesso.", new LoginResponseDto
            {
                Token = token,
                Nome = usuario.Nome,
                Role = usuario.Role,
                Expiracao = expiracao
            });
        }

        public async Task<bool> AlterarPasswordAsync(int usuarioId, string passwordAntiga, string passwordNova)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(passwordAntiga, usuario.PasswordHash))
                return false;

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordNova);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GerarToken(Usuario usuario)
        {
            var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
