using Microsoft.EntityFrameworkCore;
using VisaFlow.Data;
using VisaFlow.DTOs;

namespace VisaFlow.Services
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync();
    }

    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // ===== CLIENTES (SQL direto) =====
            var totalClientesTask = _context.Clientes.CountAsync();
            var clientesAtivosTask = _context.Clientes.CountAsync(c => c.Activo);

            // ===== PROCESSOS (carrega apenas o necessário) =====
            var processos = await _context.Processos
                .Include(p => p.Pagamentos)
                .Include(p => p.Cliente)
                .AsNoTracking()
                .ToListAsync();

            // ===== PAGAMENTOS (FIX SQLITE decimal issue) =====
            var receitaMesList = await _context.Pagamentos
                .Where(p => p.DataPagamento >= inicioMes)
                .Select(p => p.Valor)
                .ToListAsync();

            var receitaMes = receitaMesList.Sum();

            await Task.WhenAll(totalClientesTask, clientesAtivosTask);

            // ===== CÁLCULOS EM MEMÓRIA =====
            var totalRecebido = processos
                .SelectMany(p => p.Pagamentos)
                .Sum(p => p.Valor);

            var totalPendente = processos
                .Sum(p => Math.Max(0, p.ValorTotal - p.Pagamentos.Sum(pg => pg.Valor)));

            var aprovados = processos.Count(p =>
                p.Estado == "Aprovado" || p.Estado == "Concluido");

            var totalFechados = processos.Count(p =>
                p.Estado == "Aprovado" ||
                p.Estado == "Concluido" ||
                p.Estado == "Rejeitado");

            var taxaAprovacao = totalFechados > 0
                ? Math.Round((double)aprovados / totalFechados * 100, 1)
                : 0;

            // ===== CLIENTES COM DÍVIDA =====
            var clientesComDivida = processos
                .Where(p => p.Pagamentos.Sum(pg => pg.Valor) < p.ValorTotal)
                .GroupBy(p => new { p.ClienteId, Nome = p.Cliente?.Nome ?? "" })
                .Select(g => new ClienteComDividaDto
                {
                    ClienteId = g.Key.ClienteId,
                    ClienteNome = g.Key.Nome,
                    TotalDivida = g.Sum(p => p.ValorTotal - p.Pagamentos.Sum(pg => pg.Valor)),
                    NumeroProcessos = g.Count()
                })
                .OrderByDescending(x => x.TotalDivida)
                .Take(5)
                .ToList();

            // ===== ÚLTIMOS PROCESSOS =====
            var ultimosProcessos = processos
                .OrderByDescending(p => p.DataCriacao)
                .Take(5)
                .Select(ProcessoService.MapToDto)
                .ToList();

            return new DashboardDto
            {
                TotalClientes = await totalClientesTask,
                ClientesActivos = await clientesAtivosTask,

                ProcessosAbertos = processos.Count(p => p.Estado != "Concluido" && p.Estado != "Rejeitado"),
                ProcessosConcluidos = processos.Count(p => p.Estado == "Concluido"),
                ProcessosPendentes = processos.Count(p => p.Estado == "Pendente"),
                ProcessosRejeitados = processos.Count(p => p.Estado == "Rejeitado"),

                ReceitaTotal = totalRecebido,
                ReceitaPendente = totalPendente,
                ReceitaMesActual = receitaMes,

                TaxaAprovacao = taxaAprovacao,
                UltimosProcessos = ultimosProcessos,
                ClientesComDivida = clientesComDivida
            };
        }
    }
}