using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SharedKernel.Events;
using transaction_service.application.Services;
using transaction_service.domain.Entities;

namespace transaction_service.infrastructure.Kafka;

public class TransactionProducer : ITransactionProducer
{
    private readonly IProducer<string, string> _producer;
    public TransactionProducer(IOptions<KafkaSettings> kafkaSettings)
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


    public async Task PublishTransactionCreated(Transaction transaction)
    {
        var transactionCreated = new TransactionCreatedEvent(TransactionId: transaction.Id,
                SourceAccountId: transaction.SourceAccountId,
                Value: transaction.Value,
                CreatedAt: DateTime.UtcNow
        );
        await SendAsync("transaction-created", transactionCreated);
    }
}
