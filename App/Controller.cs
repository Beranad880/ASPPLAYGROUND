using Microsoft.AspNetCore.Mvc;

namespace WebApplicationASP01.App
{
    [ApiController]
    [Route("api")]  // Základní routa pro celý kontroler
    public class MyController : ControllerBase  // Lepší název než jen "Controller"
    {
        [HttpGet("ahoj")]  // Cesta bude: /api/ahoj
        public IActionResult Index()
        {
            return Ok("Hello, World!");
        }
    }
}