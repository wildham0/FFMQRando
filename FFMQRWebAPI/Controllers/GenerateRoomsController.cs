using FFMQLib;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Web;


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
        public IActionResult Get(string s, int m, bool c, bool b, int cs, bool km, bool? os = null)
        {
			//return StatusCode(StatusCodes.Status426UpgradeRequired, "Your version of AP World is unsupported, upgrade to 1.7 APWorld or set the following feature to default.");

			// cs and km are set for compatibility with 1.4; if AP is updated to 1.5 before, we can remove the default value
			FFMQRom rom = new FFMQRom();
            var newrooms = rom.GenerateRooms(c, b, m, cs, km, os, s);
            
            var bytes = Encoding.ASCII.GetBytes(newrooms);
            return File(bytes, "text/plain", "rooms.yaml");
        }
    }
}