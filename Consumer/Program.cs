using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory() { HostName = System.Configuration.ConfigurationManager.AppSettings["rabbitmq:host"] };
string queueName = System.Configuration.ConfigurationManager.AppSettings["rabbitmq:queue:sms"];
var rabbitMqConnection = factory.CreateConnection();
var rabbitMqChannel = rabbitMqConnection.CreateModel();

rabbitMqChannel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

int messageCount = Convert.ToInt16(rabbitMqChannel.MessageCount(queueName));
Console.WriteLine($"SMS waiting to send count: {messageCount}");

var consumer = new EventingBasicConsumer(rabbitMqChannel);
consumer.Received += (model, args) =>
{
    byte[] body = args.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"SMS details: {message}");
    rabbitMqChannel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
    Thread.Sleep(1000);
};
rabbitMqChannel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

Thread.Sleep(1000 * messageCount);
Console.WriteLine(" Connection closed, no more messages.");
Console.ReadLine();