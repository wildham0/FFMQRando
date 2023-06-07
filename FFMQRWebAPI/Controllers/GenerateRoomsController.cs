using Microsoft.AspNetCore.Mvc;
using System.Text;
using FFMQLib;



namespace FFMQRWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenerateRoomsController : ControllerBase
    {
        private readonly ILogger<GenerateRoomsController> _logger;

        public GenerateRoomsController(ILogger<GenerateRoomsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(string s, int m, bool c, bool b)
        {
            FFMQRom rom = new FFMQRom();
            var newrooms = rom.GenerateRooms(c, b, m, s);
            
            var bytes = Encoding.ASCII.GetBytes(newrooms);
            return File(bytes, "text/plain", "rooms.yaml");
        }
    }
}