namespace DevTech.Tickets;

using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class CreateTicket
{
    private readonly ILogger<CreateTicket> _logger;

    public CreateTicket(ILogger<CreateTicket> logger)
    {
        _logger = logger;
    }

    [Function(nameof(CreateTicket))]
    public async Task Run(
        [ServiceBusTrigger("myqueue", Connection = "development123_SERVICEBUS")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }
}