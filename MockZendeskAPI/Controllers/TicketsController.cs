using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using MockZendeskAPI.Models;

namespace MockZendeskAPI.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly Container _container;

    public TicketsController(
        CosmosClient cosmosClient,
        IConfiguration configuration)
    {
        _container = cosmosClient.GetContainer(
            configuration["CosmosDb:DatabaseName"],
            configuration["CosmosDb:ContainerName"]);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket(
        [FromBody] TicketRequest request,
        [FromQuery] string scenario = "success")
    {
        switch (scenario.ToLower())
        {
            case "timeout":
                await Task.Delay(TimeSpan.FromSeconds(35));
                break;

            case "throttle":
                Response.Headers.Append("Retry-After", "30");

                return StatusCode(429, new
                {
                    error = "Rate limit exceeded",
                    retryAfter = 30
                });

            case "servererror":
                return StatusCode(500, new
                {
                    error = "Internal Server Error"
                });

            case "duplicate":

                var duplicateQuery = new QueryDefinition(
                    "SELECT * FROM c WHERE c.subject = @subject")
                    .WithParameter("@subject", request.Subject);

                var options = new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(request.RequesterEmail)
                };

                using (FeedIterator<dynamic> iterator =
                    _container.GetItemQueryIterator<dynamic>(
                        duplicateQuery,
                        requestOptions: options))
                {
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();

                        if (response.Any())
                        {
                            return Conflict(new
                            {
                                error = "Duplicate ticket",
                                existingTicketId = response.First().id
                            });
                        }
                    }
                }

                break;
        }

        var ticketId = Guid.NewGuid().ToString();

        var ticket = new
        {
            id = ticketId,
            requesterEmail = request.RequesterEmail,
            subject = request.Subject,
            description = request.Description,
            status = "Created",
            createdAt = DateTime.UtcNow
        };

        await _container.CreateItemAsync(
            ticket,
            new PartitionKey(request.RequesterEmail));

        return Ok(new
        {
            TicketId = ticketId,
            Status = "Created",
            Message = "Ticket created successfully."
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetTickets()
    {
        var tickets = new List<dynamic>();

        using FeedIterator<dynamic> iterator =
            _container.GetItemQueryIterator<dynamic>(
                new QueryDefinition("SELECT * FROM c"));

        while (iterator.HasMoreResults)
        {
            FeedResponse<dynamic> response =
                await iterator.ReadNextAsync();

            tickets.AddRange(response);
        }

        return Ok(tickets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicket(
        string id,
        [FromQuery] string requesterEmail)
    {
        try
        {
            ItemResponse<dynamic> response =
                await _container.ReadItemAsync<dynamic>(
                    id,
                    new PartitionKey(requesterEmail));

            return Ok(response.Resource);
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound(new
            {
                error = "Ticket not found"
            });
        }
    }
}