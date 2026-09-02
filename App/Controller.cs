using Microsoft.AspNetCore.Mvc;

namespace WebApplicationASP01.App
{   

    public class Controller: ControllerBase
    {

        [HttpGet("/ahoj")]
        public IActionResult Index()
        {
            return Ok("Hello, World!");
        }
    }
}
 