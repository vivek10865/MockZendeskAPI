namespace MockZendeskAPI.Models
{
    public class TicketResponse
    {
        public int TicketId { get; set; }
        public string Status { get; set; } = "";
        public string Message { get; set; } = "";
    }
}