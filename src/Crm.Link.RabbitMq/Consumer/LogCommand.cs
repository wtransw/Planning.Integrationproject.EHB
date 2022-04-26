namespace Crm.Link.RabbitMq.Consumer
{
    public class LogCommand
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public byte[] Data { get; set; }
    }
}
