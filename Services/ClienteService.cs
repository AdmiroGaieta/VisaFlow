using Microsoft.EntityFrameworkCore;
using VisaFlow.Data;
using VisaFlow.DTOs;
using VisaFlow.Models;

namespace VisaFlow.Services
{
    public interface IClienteService
    {
        Task<List<ClienteResponseDto>> GetAllAsync(string? search, bool? activo);
        Task<ClienteResponseDto?> GetByIdAsync(int id);
        Task<ClienteResponseDto> CreateAsync(ClienteCreateDto dto);
        Task<ClienteResponseDto?> UpdateAsync(int id, ClienteUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public class ClienteService : IClienteService
    {
        private readonly AppDbContext _context;

        public ClienteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ClienteResponseDto>> GetAllAsync(string? search, bool? activo)
        {
            var query = _context.Clientes
                .Include(c => c.Processos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c =>
                    c.Nome.Contains(search) ||
                    c.Email.Contains(search) ||
                    c.Telefone.Contains(search) ||
                    c.NumeroBI.Contains(search));

            if (activo.HasValue)
                query = query.Where(c => c.Activo == activo.Value);

            return await query
                .OrderByDescending(c => c.DataCadastro)
                .Select(c => MapToDto(c))
                .ToListAsync();
        }

        public async Task<ClienteResponseDto?> GetByIdAsync(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Processos)
                .FirstOrDefaultAsync(c => c.Id == id);

            return cliente == null ? null : MapToDto(cliente);
        }

        public async Task<ClienteResponseDto> CreateAsync(ClienteCreateDto dto)
        {
            var cliente = new Cliente
            {
                Nome = dto.Nome,
                Telefone = dto.Telefone,
                Email = dto.Email,
                Nacionalidade = dto.Nacionalidade,
                NumeroBI = dto.NumeroBI,
                Observacoes = dto.Observacoes
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return MapToDto(cliente);
        }

        public async Task<ClienteResponseDto?> UpdateAsync(int id, ClienteUpdateDto dto)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Processos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null) return null;

            cliente.Nome = dto.Nome;
            cliente.Telefone = dto.Telefone;
            cliente.Email = dto.Email;
            cliente.Nacionalidade = dto.Nacionalidade;
            cliente.NumeroBI = dto.NumeroBI;
            cliente.Observacoes = dto.Observacoes;
            cliente.Activo = dto.Activo;

            await _context.SaveChangesAsync();
            return MapToDto(cliente);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return false;

            // Soft delete
            cliente.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        private static ClienteResponseDto MapToDto(Cliente c) => new()
        {
            Id = c.Id,
            Nome = c.Nome,
            Telefone = c.Telefone,
            Email = c.Email,
            Nacionalidade = c.Nacionalidade,
            NumeroBI = c.NumeroBI,
            Observacoes = c.Observacoes,
            Activo = c.Activo,
            DataCadastro = c.DataCadastro,
            TotalProcessos = c.Processos?.Count ?? 0
        };
    }
}
