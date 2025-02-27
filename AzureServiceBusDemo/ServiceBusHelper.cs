using Azure.Messaging.ServiceBus;
namespace AzureServiceBusDemo
{
    public class ServiceBusHelper
    {
        private readonly string connectionString = "Your_ServiceBus_ConnectionString";

        public async Task SendMessagesToMultipleQueuesAsync(Dictionary<string, List<string>> queueMessages)
        {
            await using var client = new ServiceBusClient(connectionString);

            foreach (var queue in queueMessages)
            {
                string queueName = queue.Key;
                List<string> messages = queue.Value;

                ServiceBusSender sender = client.CreateSender(queueName);
                using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

                foreach (var message in messages)
                {
                    if (!messageBatch.TryAddMessage(new ServiceBusMessage(message)))
                    {
                        // Send the current batch and start a new one
                        await sender.SendMessagesAsync(messageBatch);
                        messageBatch.Dispose();

                        // Create a new batch and add the remaining message
                        using ServiceBusMessageBatch newBatch = await sender.CreateMessageBatchAsync();
                        if (!newBatch.TryAddMessage(new ServiceBusMessage(message)))
                        {
                            throw new Exception("Message size too large to fit in a batch");
                        }
                    }
                }

                // Send the final batch
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"📤 Sent {messages.Count} messages to {queueName}");
            }
        }
    }
}
