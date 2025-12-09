using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Web;
using System.Reflection;
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
        public IActionResult Get(string s, int m, bool c, bool b, int cs, bool km, bool? os = null, string? version = null)
        {
			bool versionFormatError = false;
			bool unsupportedVersion = version == null;

			if (version != null)
			{
				var versions = version.Split('.');
				if (versions.Length != 2)
				{
					versionFormatError = true;
				}
				else if (versions[0] != "1" || versions[1] != "7")
				{
					unsupportedVersion = true;
				}
			}

			if (unsupportedVersion || versionFormatError)
			{
				if (os == null)
				{
					return StatusCode(StatusCodes.Status426UpgradeRequired, "Your version of FFMQ's APWorld is unsupported, upgrade to APWorld version 1.7 or set the following options to default: Map Shuffle, Crest Shuffle, Shuffle Battlefield Rewards, and Companions Locations.");
				}
				else
				{
					return StatusCode(StatusCodes.Status426UpgradeRequired, "Your version of FFMQ's APWorld is unsupported, upgrade to APWorld version 1.7 or set the following options to default: Overworld Shuffle, Map Shuffle, Crest Shuffle, Shuffle Battlefield Rewards, and Companions Locations.");
				}
			}
			else
			{
				FFMQRom rom = new FFMQRom();
				var newrooms = rom.GenerateRooms(c, b, m, cs, km, os, s);

				var bytes = Encoding.ASCII.GetBytes(newrooms);
				return File(bytes, "text/plain", "rooms.yaml");
			}
		}
    }
}