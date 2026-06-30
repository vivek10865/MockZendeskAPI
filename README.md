# Mock Zendesk Ticket API

A mock Zendesk Ticket API built using **ASP.NET Core (.NET 10)** with **Azure Cosmos DB** integration.

This project simulates common Zendesk ticket creation scenarios for testing and development without using the real Zendesk API.

---

## Features

- Create tickets
- Save tickets to Azure Cosmos DB
- Simulate successful ticket creation
- Simulate request timeout
- Simulate API throttling (429 Too Many Requests)
- Simulate duplicate ticket detection (409 Conflict)
- Simulate internal server error (500)
- Retrieve all tickets
- Retrieve ticket by ID

---

## Tech Stack

- ASP.NET Core (.NET 10)
- Azure Cosmos DB (NoSQL)
- Azure App Service (Deployment)
- REST API

---

## Project Structure

```
MockZendeskAPI
│
├── Controllers
│   └── TicketsController.cs
│
├── Models
│   ├── TicketRequest.cs
│   └── TicketResponse.cs
│
├── Program.cs
├── appsettings.json
└── README.md
```

---

## API Endpoints

### Create Ticket

```
POST /api/tickets
```

### Success

```
POST /api/tickets?scenario=success
```

Returns

```json
{
    "ticketId":"<guid>",
    "status":"Created",
    "message":"Ticket created successfully."
}
```

---

### Timeout

```
POST /api/tickets?scenario=timeout
```

Simulates a 35-second delay before returning a response.

---

### Throttle

```
POST /api/tickets?scenario=throttle
```

Returns

```
HTTP 429
```

```json
{
    "error":"Rate limit exceeded",
    "retryAfter":30
}
```

---

### Duplicate Ticket

```
POST /api/tickets?scenario=duplicate
```

First request

```
200 OK
```

Second request

```
409 Conflict
```

```json
{
    "error":"Duplicate ticket",
    "existingTicketId":"<ticket-id>"
}
```

---

### Server Error

```
POST /api/tickets?scenario=servererror
```

Returns

```
HTTP 500
```

---

### Get All Tickets

```
GET /api/tickets
```

---

### Get Ticket by ID

```
GET /api/tickets/{id}?requesterEmail=test@test.com
```

---

## Cosmos DB Configuration

Create the following resources.

Database

```
MockZendesk
```

Container

```
Tickets
```

Partition Key

```
/requesterEmail
```

---

## appsettings.json

```json
{
  "CosmosDb": {
    "ConnectionString": "<YOUR_CONNECTION_STRING>",
    "DatabaseName": "MockZendesk",
    "ContainerName": "Tickets"
  }
}
```

For production, configure these values using Azure App Service Environment Variables.

---

## Running Locally

Clone the repository

```bash
git clone https://github.com/<username>/MockZendeskAPI.git
```

Navigate into the project

```bash
cd MockZendeskAPI
```

Restore packages

```bash
dotnet restore
```

Run the application

```bash
dotnet run
```

---

## Testing

Example request

```powershell
Invoke-RestMethod `
-Uri "http://localhost:5217/api/tickets?scenario=success" `
-Method Post `
-ContentType "application/json" `
-Body '{
"subject":"Azure Test",
"description":"Testing Cosmos DB",
"requesterEmail":"test@test.com"
}'
```

---

## Deployment

This project can be deployed to Azure App Service.

Configure the following Environment Variables:

```
CosmosDb__ConnectionString
CosmosDb__DatabaseName
CosmosDb__ContainerName
```

---

## Future Enhancements

- Swagger/OpenAPI documentation
- Authentication
- Ticket comments
- Ticket updates
- Pagination
- Feedback API
- Search by requester email
- Randomized latency and failures

---

## Author

Vivek
