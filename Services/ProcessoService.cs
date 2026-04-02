using Microsoft.EntityFrameworkCore;
using VisaFlow.Data;
using VisaFlow.DTOs;
using VisaFlow.Models;

namespace VisaFlow.Services
{
    public interface IProcessoService
    {
        Task<List<ProcessoResponseDto>> GetAllAsync(int? clienteId, string? estado, string? tipo);
        Task<ProcessoResponseDto?> GetByIdAsync(int id);
        Task<ProcessoResponseDto> CreateAsync(ProcessoCreateDto dto, string criadoPor);
        Task<ProcessoResponseDto?> UpdateAsync(int id, ProcessoUpdateDto dto);
        Task<bool> AlterarEstadoAsync(int id, string novoEstado);
        Task<bool> DeleteAsync(int id);
    }

    public class ProcessoService : IProcessoService
    {
        private readonly AppDbContext _context;

        public ProcessoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProcessoResponseDto>> GetAllAsync(int? clienteId, string? estado, string? tipo)
        {
            var query = _context.Processos
                .Include(p => p.Cliente)
                .Include(p => p.Pagamentos)
                .Include(p => p.Documentos)
                .AsQueryable();

            if (clienteId.HasValue)
                query = query.Where(p => p.ClienteId == clienteId.Value);

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(p => p.Estado == estado);

            if (!string.IsNullOrWhiteSpace(tipo))
                query = query.Where(p => p.Tipo == tipo);

            var processos = await query
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();

            return processos.Select(MapToDto).ToList();
        }

        public async Task<ProcessoResponseDto?> GetByIdAsync(int id)
        {
            var processo = await _context.Processos
                .Include(p => p.Cliente)
                .Include(p => p.Pagamentos)
                .Include(p => p.Documentos)
                .FirstOrDefaultAsync(p => p.Id == id);

            return processo == null ? null : MapToDto(processo);
        }

        public async Task<ProcessoResponseDto> CreateAsync(ProcessoCreateDto dto, string criadoPor)
        {
            var processo = new Processo
            {
                ClienteId = dto.ClienteId,
                Tipo = dto.Tipo,
                PaisDestino = dto.PaisDestino,
                ValorTotal = dto.ValorTotal,
                Estado = "Pendente",
                Observacoes = dto.Observacoes,
                DataAgendamento = dto.DataAgendamento,
                CriadoPor = criadoPor
            };

            _context.Processos.Add(processo);
            await _context.SaveChangesAsync();

            // Recarregar com relacionamentos
            return (await GetByIdAsync(processo.Id))!;
        }

        public async Task<ProcessoResponseDto?> UpdateAsync(int id, ProcessoUpdateDto dto)
        {
            var processo = await _context.Processos
                .Include(p => p.Cliente)
                .Include(p => p.Pagamentos)
                .Include(p => p.Documentos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (processo == null) return null;

            processo.Tipo = dto.Tipo;
            processo.PaisDestino = dto.PaisDestino;
            processo.ValorTotal = dto.ValorTotal;
            processo.Estado = dto.Estado;
            processo.Observacoes = dto.Observacoes;
            processo.DataAgendamento = dto.DataAgendamento;

            if (dto.Estado == "Concluido" && processo.DataConclusao == null)
                processo.DataConclusao = DateTime.Now;

            await _context.SaveChangesAsync();
            return MapToDto(processo);
        }

        public async Task<bool> AlterarEstadoAsync(int id, string novoEstado)
        {
            var processo = await _context.Processos.FindAsync(id);
            if (processo == null) return false;

            processo.Estado = novoEstado;

            if (novoEstado == "Concluido")
                processo.DataConclusao = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var processo = await _context.Processos.FindAsync(id);
            if (processo == null) return false;

            _context.Processos.Remove(processo);
            await _context.SaveChangesAsync();
            return true;
        }

        public static ProcessoResponseDto MapToDto(Processo p)
        {
            var totalPago = p.Pagamentos?.Sum(pg => pg.Valor) ?? 0;
            var saldo = p.ValorTotal - totalPago;
            var percentagem = p.ValorTotal > 0 ? Math.Round((totalPago / p.ValorTotal) * 100, 2) : 0;

            string estadoPagamento = saldo == 0 ? "Pago"
                                   : totalPago == 0 ? "Pendente"
                                   : "Parcial";

            var docs = p.Documentos ?? new List<Documento>();

            return new ProcessoResponseDto
            {
                Id = p.Id,
                ClienteId = p.ClienteId,
                ClienteNome = p.Cliente?.Nome ?? string.Empty,
                Tipo = p.Tipo,
                PaisDestino = p.PaisDestino,
                ValorTotal = p.ValorTotal,
                Estado = p.Estado,
                Observacoes = p.Observacoes,
                DataCriacao = p.DataCriacao,
                DataAgendamento = p.DataAgendamento,
                DataConclusao = p.DataConclusao,
                TotalPago = totalPago,
                Saldo = saldo,
                PercentagemPaga = percentagem,
                EstadoPagamento = estadoPagamento,
                TotalDocumentos = docs.Count,
                DocumentosEntregues = docs.Count(d => d.Status == "Entregue"),
                DocumentosPendentes = docs.Count(d => d.Status == "Pendente"),
                DocumentosInvalidos = docs.Count(d => d.Status == "Invalido")
            };
        }
    }
}
