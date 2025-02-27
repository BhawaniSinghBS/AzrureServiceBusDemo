using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureServiceBusDemo
{
    public class WorkerReceiverFromBus : BackgroundService
    {
        private readonly string connectionString = "ServiceBus_ConnectionString";
        private readonly List<string> queueNames = new List<string> { "orders-queue", "payments-queue", "notifications-queue" };
        private readonly ILogger<WorkerReceiverFromBus> _logger;
        private readonly Dictionary<string, ServiceBusProcessor> _processors = new();

        public WorkerReceiverFromBus(ILogger<WorkerReceiverFromBus> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Message Processor is running...");

            var client = new ServiceBusClient(connectionString);

            foreach (var queueName in queueNames)
            {
                var processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

                processor.ProcessMessageAsync += async args =>
                {
                    string body = args.Message.Body.ToString();
                    _logger.LogInformation($"📩 Received from [{queueName}]: {body}");

                    await args.CompleteMessageAsync(args.Message);
                };

                processor.ProcessErrorAsync += args =>
                {
                    _logger.LogError($"❌ Error in [{queueName}]: {args.Exception.Message}");
                    return Task.CompletedTask;
                };

                _processors[queueName] = processor;
                await processor.StartProcessingAsync();
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🛑 Stopping message processors...");

            foreach (var processor in _processors.Values)
            {
                await processor.StopProcessingAsync();
            }
        }
    }
}
