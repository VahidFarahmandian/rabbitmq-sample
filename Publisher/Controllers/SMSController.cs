using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Publisher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SMSController : ControllerBase
    {
        public SMSController(IConfiguration configuration) => Configuration = configuration;

        private IConfiguration Configuration { get; }

        [HttpPost]
        public void Post(SMS message)
        {
            string queue = Configuration["rabbitmq:queue:sms"];
            var factory = new ConnectionFactory() { HostName = Configuration["rabbitmq:host"] };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { Message = message, SendRequestDate = DateTime.Now }));

            channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
        }
    }
}