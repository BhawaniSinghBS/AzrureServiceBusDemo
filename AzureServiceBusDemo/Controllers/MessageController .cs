using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureServiceBusDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly ServiceBusHelper _serviceBusHelper;

        public MessageController()
        {
            _serviceBusHelper = new ServiceBusHelper();
        }

        // Send a single message to a specific queue
        [HttpPost("send/{queueName}")]
        public async Task<IActionResult> SendMessage(string queueName, [FromBody] string message)
        {
            var queueMessages = new Dictionary<string, List<string>>
            {
                { queueName, new List<string> { message } }
            };

            await _serviceBusHelper.SendMessagesToMultipleQueuesAsync(queueMessages);
            return Ok($"Sent message to {queueName}: {message}");
        }

        // Send multiple messages to multiple queues
        [HttpPost("sendMultiple")]
        public async Task<IActionResult> SendMultipleMessages([FromBody] Dictionary<string, List<string>> queueMessages)
        {
            await _serviceBusHelper.SendMessagesToMultipleQueuesAsync(queueMessages);
            return Ok("Messages sent to multiple queues successfully.");
        }
    }
}
