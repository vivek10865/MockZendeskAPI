namespace MockZendeskAPI.Models
{
    public class TicketRequest
    {
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
        public string RequesterEmail { get; set; } = "";
    }
}