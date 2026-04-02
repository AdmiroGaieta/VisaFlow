using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VisaFlow.DTOs;
using VisaFlow.Services;

namespace VisaFlow.Controllers
{
    [ApiController]
    [Route("api/pagamentos")]
    [Authorize]
    public class PagamentosController : ControllerBase
    {
        private readonly IFinanceiroService _service;

        public PagamentosController(IFinanceiroService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retorna o resumo financeiro completo de um processo:
        /// valor total, total pago, saldo, percentagem e histórico.
        /// </summary>
        [HttpGet("resumo/{processoId}")]
        public async Task<IActionResult> GetResumo(int processoId)
        {
            var resumo = await _service.GetResumoAsync(processoId);
            if (resumo == null) return NotFound(new { mensagem = "Processo não encontrado." });
            return Ok(resumo);
        }

        /// <summary>
        /// Lista todos os pagamentos de um processo.
        /// </summary>
        [HttpGet("processo/{processoId}")]
        public async Task<IActionResult> GetByProcesso(int processoId)
        {
            var pagamentos = await _service.GetPagamentosByProcessoAsync(processoId);
            return Ok(pagamentos);
        }

        /// <summary>
        /// Regista um pagamento (pode ser parcial).
        /// Validação automática: não permite pagar mais que o saldo em dívida.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Registar([FromBody] PagamentoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var registadoPor = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
            var (sucesso, mensagem, pagamento) = await _service.RegistarPagamentoAsync(dto, registadoPor);

            if (!sucesso) return BadRequest(new { mensagem });
            return Ok(new { mensagem, pagamento });
        }

        /// <summary>
        /// Elimina um pagamento (apenas Admin).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var sucesso = await _service.EliminarPagamentoAsync(id);
            if (!sucesso) return NotFound(new { mensagem = "Pagamento não encontrado." });
            return Ok(new { mensagem = "Pagamento eliminado com sucesso." });
        }

        /// <summary>
        /// Dashboard financeiro com receita total, pendente e clientes com dívida.
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim)
        {
            var dashboard = await _service.GetDashboardFinanceiroAsync(dataInicio, dataFim);
            return Ok(dashboard);
        }
    }
}
