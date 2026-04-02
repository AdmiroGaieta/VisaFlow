using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisaFlow.DTOs;
using VisaFlow.Services;

namespace VisaFlow.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _service;

        public ClientesController(IClienteService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lista todos os clientes. Suporta filtros por nome/email/BI e estado activo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? activo)
        {
            var clientes = await _service.GetAllAsync(search, activo);
            return Ok(clientes);
        }

        /// <summary>
        /// Retorna um cliente pelo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cliente = await _service.GetByIdAsync(id);
            if (cliente == null) return NotFound(new { mensagem = "Cliente não encontrado." });
            return Ok(cliente);
        }

        /// <summary>
        /// Cria um novo cliente.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClienteCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var cliente = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
        }

        /// <summary>
        /// Actualiza os dados de um cliente.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ClienteUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var cliente = await _service.UpdateAsync(id, dto);
            if (cliente == null) return NotFound(new { mensagem = "Cliente não encontrado." });
            return Ok(cliente);
        }

        /// <summary>
        /// Desactiva (soft delete) um cliente.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var sucesso = await _service.DeleteAsync(id);
            if (!sucesso) return NotFound(new { mensagem = "Cliente não encontrado." });
            return Ok(new { mensagem = "Cliente desactivado com sucesso." });
        }
    }
}
