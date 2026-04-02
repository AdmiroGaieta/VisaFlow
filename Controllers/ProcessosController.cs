using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VisaFlow.DTOs;
using VisaFlow.Services;

namespace VisaFlow.Controllers
{
    [ApiController]
    [Route("api/processos")]
    [Authorize]
    public class ProcessosController : ControllerBase
    {
        private readonly IProcessoService _service;

        public ProcessosController(IProcessoService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lista processos. Filtra por clienteId, estado ou tipo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? clienteId,
            [FromQuery] string? estado,
            [FromQuery] string? tipo)
        {
            var processos = await _service.GetAllAsync(clienteId, estado, tipo);
            return Ok(processos);
        }

        /// <summary>
        /// Retorna um processo com resumo financeiro e de documentos.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var processo = await _service.GetByIdAsync(id);
            if (processo == null) return NotFound(new { mensagem = "Processo não encontrado." });
            return Ok(processo);
        }

        /// <summary>
        /// Cria um novo processo para um cliente.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProcessoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var criadoPor = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
            var processo = await _service.CreateAsync(dto, criadoPor);
            return CreatedAtAction(nameof(GetById), new { id = processo.Id }, processo);
        }

        /// <summary>
        /// Actualiza os dados de um processo.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProcessoUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var processo = await _service.UpdateAsync(id, dto);
            if (processo == null) return NotFound(new { mensagem = "Processo não encontrado." });
            return Ok(processo);
        }

        /// <summary>
        /// Altera apenas o estado de um processo.
        /// Ex: PATCH /api/processos/5/estado  body: { "estado": "Aprovado" }
        /// </summary>
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> AlterarEstado(int id, [FromBody] AlterarEstadoDto dto)
        {
            var estadosValidos = new[] { "Pendente", "EmAnalise", "Agendado", "Aprovado", "Rejeitado", "Concluido" };
            if (!estadosValidos.Contains(dto.Estado))
                return BadRequest(new { mensagem = "Estado inválido." });

            var sucesso = await _service.AlterarEstadoAsync(id, dto.Estado);
            if (!sucesso) return NotFound(new { mensagem = "Processo não encontrado." });
            return Ok(new { mensagem = $"Estado alterado para '{dto.Estado}'." });
        }

        /// <summary>
        /// Elimina um processo (apenas Admin).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var sucesso = await _service.DeleteAsync(id);
            if (!sucesso) return NotFound(new { mensagem = "Processo não encontrado." });
            return Ok(new { mensagem = "Processo eliminado com sucesso." });
        }
    }

    public class AlterarEstadoDto
    {
        public string Estado { get; set; } = string.Empty;
    }
}
