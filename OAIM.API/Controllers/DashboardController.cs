using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OAIM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
           _dashboardService = dashboardService;
        }
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _dashboardService.GetDashboard();

            return Ok(dashboard);
        }
    }
}
