using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FFMQRWebAPI
{
    /*
    public class GenerateRoomsController : ControllerBase
    {
        [HttpGet("yaml-text")]
        public HttpResponseMessage ReturnByteArray()
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
        }
    }*/
    

        /*
        [ApiController]
        public class DownloadsController : ControllerBase
        {
            public readonly IFileService _fileService;

            [HttpGet("yaml-text")]
            public IActionResult ReturnByteArray()
            { 




            }
        }*/
    }