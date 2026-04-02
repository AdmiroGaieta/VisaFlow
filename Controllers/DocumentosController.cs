using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VisaFlow.DTOs;
using VisaFlow.Services;

namespace VisaFlow.Controllers
{
    [ApiController]
    [Route("api/documentos")]
    [Authorize]
    public class DocumentosController : ControllerBase
    {
        private readonly IDocumentoService _service;

        public DocumentosController(IDocumentoService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lista todos os documentos de um processo.
        /// </summary>
        [HttpGet("processo/{processoId}")]
        public async Task<IActionResult> GetByProcesso(int processoId)
        {
            var docs = await _service.GetByProcessoAsync(processoId);
            return Ok(docs);
        }

        /// <summary>
        /// Retorna o checklist de documentos obrigatórios por tipo de processo.
        /// Ex: GET /api/documentos/checklist/VistoTrabalho
        /// </summary>
        [HttpGet("checklist/{tipoProcesso}")]
        public IActionResult GetChecklist(string tipoProcesso)
        {
            var checklist = _service.GetChecklistPorTipo(tipoProcesso);
            return Ok(new { tipoProcesso, documentos = checklist });
        }

        /// <summary>
        /// Upload de documento para um processo.
        /// Formato: multipart/form-data. Campos: ficheiro (IFormFile), tipo (string).
        /// </summary>
        [HttpPost("upload/{processoId}")]
        public async Task<IActionResult> Upload(
            int processoId,
            IFormFile ficheiro,
            [FromForm] string tipo)
        {
            if (ficheiro == null || ficheiro.Length == 0)
                return BadRequest(new { mensagem = "Nenhum ficheiro enviado." });

            var carregadoPor = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
            var (sucesso, mensagem, doc) = await _service.UploadAsync(processoId, ficheiro, tipo, carregadoPor);

            if (!sucesso) return BadRequest(new { mensagem });
            return Ok(new { mensagem, documento = doc });
        }

        /// <summary>
        /// Altera o estado de um documento (Entregue / Invalido / Pendente).
        /// Usado pelo operador para validar ou rejeitar um documento.
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> AlterarStatus(int id, [FromBody] DocumentoUpdateStatusDto dto)
        {
            var sucesso = await _service.AlterarStatusAsync(id, dto);
            if (!sucesso) return NotFound(new { mensagem = "Documento não encontrado." });
            return Ok(new { mensagem = $"Estado do documento alterado para '{dto.Status}'." });
        }

        /// <summary>
        /// Download de um documento.
        /// </summary>
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var resultado = await _service.DownloadAsync(id);
            if (resultado == null) return NotFound(new { mensagem = "Documento não encontrado ou ficheiro em falta." });

            var (bytes, contentType, nome) = resultado.Value;
            return File(bytes, contentType, nome);
        }

        /// <summary>
        /// Elimina um documento.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var sucesso = await _service.EliminarAsync(id);
            if (!sucesso) return NotFound(new { mensagem = "Documento não encontrado." });
            return Ok(new { mensagem = "Documento eliminado com sucesso." });
        }
    }
}
