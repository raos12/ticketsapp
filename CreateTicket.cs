namespace DevTech.Tickets;

using System;
using System.Configuration;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;

public class CreateTicket
{
    private readonly ILogger<CreateTicket> _logger;

    public CreateTicket(ILogger<CreateTicket> logger)
    {
        _logger = logger;
    }

    [Function(nameof(CreateTicket))]
    public async Task Run(
        [ServiceBusTrigger("create-ticket", Connection = "manage_tickets")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        var cosmosDbConnectionString = Environment.GetEnvironmentVariable("COSMOSDB_CONNECTION_STRING");
        var cosmosDbAccount = Environment.GetEnvironmentVariable("COSMOSDB_ACCOUNT");
        var cosmosDbKey = Environment.GetEnvironmentVariable("COSMOSDB_KEY");
        var cosmosDatabaseName = Environment.GetEnvironmentVariable("COSMOSDB_DATABASE");
        var cosmosDbContainer = Environment.GetEnvironmentVariable("COSMOSDB_CONTAINER");

        var request = message.Body.ToObjectFromJson<TicketRequest>();
        var ticket = new Ticket(message.MessageId, request.Title, request.Description, request.Priority);
        var cosmosDBClient = new CosmosClient(cosmosDbAccount, cosmosDbKey);
        
        try{
            await cosmosDBClient.GetDatabase(cosmosDatabaseName)
                .GetContainer(cosmosDbContainer)
                .CreateItemAsync(ticket, cancellationToken: new CancellationTokenSource(5000).Token);
            
            _logger.LogInformation("Function completed...");

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving data.");
            _logger.LogInformation(e.Message);
            await messageActions.AbandonMessageAsync(message);
        }      
    }
}

public record Ticket(
    string id,
    string Title,
    string Description,
    int Priority
);

public record TicketRequest(
    string Title,
    string Description,
    int Priority
);