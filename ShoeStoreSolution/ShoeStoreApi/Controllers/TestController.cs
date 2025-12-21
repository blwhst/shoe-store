using Microsoft.AspNetCore.Mvc;

namespace ShoeStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // Тестовый метод для проверки работы API
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "API работает!" });
        }
    }
}