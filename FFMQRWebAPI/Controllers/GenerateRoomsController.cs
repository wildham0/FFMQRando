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
        /*
        [HttpGet(Name = "yaml-text")]
        public IEnumerable<string> Get() { 
        
            return new List<string> { "Hello!", "Hello!" };
        }*/

        [HttpGet(Name = "yaml-text/{s:string}")]
        public IActionResult Get(string s, int m, bool c)
        {
            FFMQRom rom = new FFMQRom();
            var newrooms = rom.GenerateRooms(c, m, s);
            
            //var text = new List<string> { "Hello!", "Hello!", s };
            //var bytes = Encoding.ASCII.GetBytes(string.Join("\n", text));
            var bytes = Encoding.ASCII.GetBytes(newrooms);
            return File(bytes, "text/plain", "rooms.yaml");
        }
        /*
        public IActionResult Get()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("Hello! Hello!");
            writer.Flush();
            stream.Position = 0;

            var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(stream.ToArray())
            };

            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = "test.txt"
            };

            result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            return result;
        }*/
    }
}