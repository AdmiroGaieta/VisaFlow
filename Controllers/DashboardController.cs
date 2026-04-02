using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisaFlow.Services;

namespace VisaFlow.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retorna métricas gerais: clientes, processos, receita, taxa de aprovação.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var dashboard = await _service.GetDashboardAsync();
            return Ok(dashboard);
        }
    }
}
