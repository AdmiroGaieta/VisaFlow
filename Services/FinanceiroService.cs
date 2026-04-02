using Microsoft.EntityFrameworkCore;
using VisaFlow.Data;
using VisaFlow.DTOs;
using VisaFlow.Models;

namespace VisaFlow.Services
{
    public interface IFinanceiroService
    {
        Task<ResumoFinanceiroDto?> GetResumoAsync(int processoId);
        Task<(bool sucesso, string mensagem, PagamentoResponseDto? pagamento)> RegistarPagamentoAsync(PagamentoCreateDto dto, string registadoPor);
        Task<bool> EliminarPagamentoAsync(int pagamentoId);
        Task<List<PagamentoResponseDto>> GetPagamentosByProcessoAsync(int processoId);
        Task<DashboardFinanceiroDto> GetDashboardFinanceiroAsync(DateTime? dataInicio, DateTime? dataFim);
    }

    public class FinanceiroService : IFinanceiroService
    {
        private readonly AppDbContext _context;

        public FinanceiroService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ResumoFinanceiroDto?> GetResumoAsync(int processoId)
        {
            var processo = await _context.Processos
                .Include(p => p.Cliente)
                .Include(p => p.Pagamentos)
                .FirstOrDefaultAsync(p => p.Id == processoId);

            if (processo == null) return null;

            var totalPago = processo.Pagamentos.Sum(p => p.Valor);
            var saldo = processo.ValorTotal - totalPago;
            var percentagem = processo.ValorTotal > 0
                ? Math.Round((totalPago / processo.ValorTotal) * 100, 2)
                : 0;

            return new ResumoFinanceiroDto
            {
                ProcessoId = processo.Id,
                ClienteNome = processo.Cliente?.Nome ?? string.Empty,
                TipoProcesso = processo.Tipo,
                ValorTotal = processo.ValorTotal,
                TotalPago = totalPago,
                Saldo = saldo,
                PercentagemPaga = percentagem,
                EstadoPagamento = saldo == 0 ? "Pago" : totalPago == 0 ? "Pendente" : "Parcial",
                HistoricoPagamentos = processo.Pagamentos
                    .OrderByDescending(p => p.DataPagamento)
                    .Select(p => new PagamentoResponseDto
                    {
                        Id = p.Id,
                        ProcessoId = p.ProcessoId,
                        ClienteNome = processo.Cliente?.Nome ?? string.Empty,
                        Valor = p.Valor,
                        Metodo = p.Metodo,
                        Observacao = p.Observacao,
                        Referencia = p.Referencia,
                        DataPagamento = p.DataPagamento,
                        RegistadoPor = p.RegistadoPor
                    })
                    .ToList()
            };
        }

        public async Task<(bool sucesso, string mensagem, PagamentoResponseDto? pagamento)> RegistarPagamentoAsync(PagamentoCreateDto dto, string registadoPor)
        {
            var processo = await _context.Processos
                .Include(p => p.Cliente)
                .Include(p => p.Pagamentos)
                .FirstOrDefaultAsync(p => p.Id == dto.ProcessoId);

            if (processo == null)
                return (false, "Processo não encontrado.", null);

            if (dto.Valor <= 0)
                return (false, "O valor do pagamento deve ser maior que zero.", null);

            var saldoActual = processo.ValorTotal - processo.Pagamentos.Sum(p => p.Valor);

            if (dto.Valor > saldoActual)
                return (false, $"O valor inserido ({dto.Valor:F2} Kz) excede o saldo em dívida ({saldoActual:F2} Kz).", null);

            var pagamento = new Pagamento
            {
                ProcessoId = dto.ProcessoId,
                Valor = dto.Valor,
                Metodo = dto.Metodo,
                Observacao = dto.Observacao,
                Referencia = dto.Referencia,
                RegistadoPor = registadoPor,
                DataPagamento = DateTime.Now
            };

            _context.Pagamentos.Add(pagamento);
            await _context.SaveChangesAsync();

            var responseDto = new PagamentoResponseDto
            {
                Id = pagamento.Id,
                ProcessoId = pagamento.ProcessoId,
                ClienteNome = processo.Cliente?.Nome ?? string.Empty,
                Valor = pagamento.Valor,
                Metodo = pagamento.Metodo,
                Observacao = pagamento.Observacao,
                Referencia = pagamento.Referencia,
                DataPagamento = pagamento.DataPagamento,
                RegistadoPor = pagamento.RegistadoPor
            };

            return (true, "Pagamento registado com sucesso.", responseDto);
        }

        public async Task<bool> EliminarPagamentoAsync(int pagamentoId)
        {
            var pagamento = await _context.Pagamentos.FindAsync(pagamentoId);
            if (pagamento == null) return false;

            _context.Pagamentos.Remove(pagamento);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PagamentoResponseDto>> GetPagamentosByProcessoAsync(int processoId)
        {
            return await _context.Pagamentos
                .Include(p => p.Processo)
                    .ThenInclude(pr => pr.Cliente)
                .Where(p => p.ProcessoId == processoId)
                .OrderByDescending(p => p.DataPagamento)
                .Select(p => new PagamentoResponseDto
                {
                    Id = p.Id,
                    ProcessoId = p.ProcessoId,
                    ClienteNome = p.Processo.Cliente.Nome,
                    Valor = p.Valor,
                    Metodo = p.Metodo,
                    Observacao = p.Observacao,
                    Referencia = p.Referencia,
                    DataPagamento = p.DataPagamento,
                    RegistadoPor = p.RegistadoPor
                })
                .ToListAsync();
        }

        public async Task<DashboardFinanceiroDto> GetDashboardFinanceiroAsync(DateTime? dataInicio, DateTime? dataFim)
        {
            var inicio = dataInicio ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fim = dataFim ?? DateTime.Now;

            var processos = await _context.Processos
                .Include(p => p.Pagamentos)
                .Include(p => p.Cliente)
                .ToListAsync();

            var pagamentosNoPeriodo = await _context.Pagamentos
                .Where(p => p.DataPagamento >= inicio && p.DataPagamento <= fim)
                .ToListAsync();

            var totalRecebido = processos.Sum(p => p.Pagamentos.Sum(pg => pg.Valor));
            var totalPendente = processos.Sum(p => p.ValorTotal - p.Pagamentos.Sum(pg => pg.Valor));
            var receitaNoPeriodo = pagamentosNoPeriodo.Sum(p => p.Valor);

            var clientesComDivida = processos
                .Where(p => p.Pagamentos.Sum(pg => pg.Valor) < p.ValorTotal)
                .GroupBy(p => new { p.ClienteId, p.Cliente?.Nome })
                .Select(g => new ClienteComDividaDto
                {
                    ClienteId = g.Key.ClienteId,
                    ClienteNome = g.Key.Nome ?? string.Empty,
                    TotalDivida = g.Sum(p => p.ValorTotal - p.Pagamentos.Sum(pg => pg.Valor)),
                    NumeroProcessos = g.Count()
                })
                .OrderByDescending(c => c.TotalDivida)
                .ToList();

            return new DashboardFinanceiroDto
            {
                TotalRecebido = totalRecebido,
                TotalPendente = totalPendente,
                ReceitaNoPeriodo = receitaNoPeriodo,
                ClientesComDivida = clientesComDivida,
                DataInicio = inicio,
                DataFim = fim
            };
        }
    }

    public class DashboardFinanceiroDto
    {
        public decimal TotalRecebido { get; set; }
        public decimal TotalPendente { get; set; }
        public decimal ReceitaNoPeriodo { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public List<ClienteComDividaDto> ClientesComDivida { get; set; } = new();
    }
}
