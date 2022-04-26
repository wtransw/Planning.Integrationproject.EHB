namespace Crm.Link.RabbitMq.Producer
{
    public class IntegrationEvent
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public byte[] Data { get; set; }
    }
}
