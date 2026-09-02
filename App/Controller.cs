using Microsoft.AspNetCore.Mvc;

namespace WebApplicationASP01.App
{
    [ApiController]
    [Route("api")]  
    public class MyController : ControllerBase  
    {
        [HttpGet("ahoj")]  
        public IActionResult Index()
        {
            return Ok("Hello, World!");
        }
    }
}