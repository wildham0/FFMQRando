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
        public IActionResult Get(string s, int m, bool c, bool b, int cs, bool km, bool? os = null)
        {
			//return StatusCode(StatusCodes.Status426UpgradeRequired, "Your version of AP World is unsupported, upgrade to 1.7 APWorld or set the following feature to default.");

			Assembly ffmqlib = Assembly.LoadFrom("dll/1.6/FFMQRLib.dll");

			Type? libType = ffmqlib.GetType("FFMQLib.FFMQRom");
			if (libType != null)
			{
				object? instance = Activator.CreateInstance(libType);
				if (instance != null)
				{
					MethodInfo? method = libType.GetMethod("GenerateRooms");
					if (method != null)
					{
						string? newrooms = (string?)method.Invoke(instance, new object?[] { c, b, m, cs, km, os, s });

						if (newrooms != null)
						{
							var bytes = Encoding.ASCII.GetBytes(newrooms);
							return File(bytes, "text/plain", "rooms.yaml");
						}
					}
				}
			}

			return StatusCode(StatusCodes.Status424FailedDependency, "Legacy version failed to load. DDL Error.");
		}
    }
	[ApiController]
	[Route("[controller]")]
	public class GetRooms17Controller : ControllerBase
	{
		private readonly ILogger<GetRooms17Controller> _logger;

		public GetRooms17Controller(ILogger<GetRooms17Controller> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		public IActionResult Get(string s, int m, bool c, bool b, int cs, bool km, bool os)
		{
			FFMQRom rom = new FFMQRom();
			var newrooms = rom.GenerateRooms(c, b, m, cs, km, os, s);

			var bytes = Encoding.ASCII.GetBytes(newrooms);
			return File(bytes, "text/plain", "rooms.yaml");
		}
	}
}