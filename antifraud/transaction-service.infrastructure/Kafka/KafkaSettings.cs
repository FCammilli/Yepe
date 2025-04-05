namespace transaction_service.infrastructure.Kafka;

public class KafkaSettings
{
    public required string BootstrapServers { get; set; }
    public required string GroupId { get; set; }
}
