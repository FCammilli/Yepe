using antifraud_service.application.Services;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SharedKernel.Events;

namespace antifraud_service.infrastructure.Kafka;

public class TransactionAntiFraudProducer : IAntifraudTransactionProducer
{
    private readonly IProducer<string, string> _producer;
    public TransactionAntiFraudProducer(IOptions<KafkaSettings> kafkaSettings)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = kafkaSettings.Value.BootstrapServers
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }
    public async Task SendAsync<T>(string topic, T message)
    {

        var serializedMessage = JsonConvert.SerializeObject(message);

        await _producer.ProduceAsync(topic, new Message<string, string>
        {

            Value = serializedMessage
        });
        Console.WriteLine($"Message sent to the topic {topic}: {serializedMessage}");
    }

    public async Task PublishStatusRejectedAsync(Guid transactionId, decimal value)
    {
        var statusUpdateEvent = new TransactionStatusUpdateEvent(TransactionId: transactionId,
              Value: value,
              ProcessedAt: DateTime.UtcNow, Status: SharedKernel.Enums.TransactionStatusEvent.Rejected
        );
        await SendAsync("transaction-updated", statusUpdateEvent);
    }

    public async Task PublishStatusApprovedAsync(Guid transactionId, decimal value)
    {
        var statusUpdateEvent = new TransactionStatusUpdateEvent(TransactionId: transactionId,
              Value: value,
              ProcessedAt: DateTime.UtcNow, Status: SharedKernel.Enums.TransactionStatusEvent.Approved
        );
        await SendAsync("transaction-updated", statusUpdateEvent);
    }
}
