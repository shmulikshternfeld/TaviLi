using Microsoft.AspNetCore.Mvc;
using TaviLi.Infrastructure.Persistence;

namespace TaviLi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestDataController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _env;

        public TestDataController(IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            _serviceProvider = serviceProvider;
            _env = env;
        }

        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            // Allow this only in development environment for safety
            if (!_env.IsDevelopment())
            {
                return Forbid();
            }

            try
            {
                await SeedData.InitializeAsync(_serviceProvider);
                return Ok(new { message = "Database seeded successfully with test data! 🚀" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
