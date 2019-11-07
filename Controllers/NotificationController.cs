using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace sse.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private static ConcurrentBag<StreamWriter> clients;
        static NotificationController()
        {
            clients = new ConcurrentBag<StreamWriter>();
        }

        private readonly ILogger<NotificationController> _logger;

        public NotificationController(ILogger<NotificationController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task Get()
        {
            string name = Request.Query["name"].ToString();
            var response = Response;
            response.ContentType = "text/event-stream";
            response.Headers.Add("Connection", "Keep-Alive");
            response.Headers.Add("Keep-Alive", "timeout=120, max=10000");

            clients.Add(new StreamWriter(response.Body));
            // heartbeat, this runs forever until connection dies
            for (var i = 0; true; ++i)
            {
                byte[] s = Encoding.UTF8.GetBytes($"{name}: Controller {i} at {DateTime.Now}\n\r");
                try
                {
                    await response.Body.WriteAsync(s);
                    await response.Body.FlushAsync();
                }
                catch (Exception)
                {
                    await response.CompleteAsync();

                }
                await Task.Delay(90 * 1000);
            }
        }

        [HttpPost]
        public async Task Post()
        {
            var response = Response;
            response.StatusCode = 200;
            await response.CompleteAsync();
            // Sending to ALL clients here to simulate high load.
            // Normally, only the clients getting message should be sent to
            // and we should keep an id-clients mapping
            foreach (var client in clients)
            {
                try
                {
                    var message = "test";
                    await client.WriteAsync(message);
                    await client.FlushAsync();
                }
                catch (Exception)
                {
                    StreamWriter ignore;
                    clients.TryTake(out ignore);
                }
            }
        }
    }
}
