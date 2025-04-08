using antifraud.domain.Enums;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using transaction_service.domain.Entities;
using transaction_service.infrastructure.Kafka;
namespace transaction_service.test.infraestructura.kafka;
public class TransactionProducerTests
{
    private readonly Mock<IOptions<KafkaSettings>> _mockKafkaSettings;
    private readonly Mock<IProducer<string, string>> _mockProducer;
    private readonly TransactionProducer _transactionProducer;

    public TransactionProducerTests()
    {
        _mockKafkaSettings = new Mock<IOptions<KafkaSettings>>();
        _mockProducer = new Mock<IProducer<string, string>>();

        var kafkaSettings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group"
        };

        _mockKafkaSettings.Setup(x => x.Value).Returns(kafkaSettings);

        // Mock ProducerBuilder to return the mocked producer
        var mockProducerBuilder = new Mock<ProducerBuilder<string, string>>(It.IsAny<ProducerConfig>());
        mockProducerBuilder.Setup(x => x.Build()).Returns(_mockProducer.Object);

        // Use reflection to set the private _producer field
        _transactionProducer = new TransactionProducer(_mockKafkaSettings.Object);
        var producerField = typeof(TransactionProducer).GetField("_producer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        producerField.SetValue(_transactionProducer, _mockProducer.Object);
    }

    [Fact]
    public async Task SendAsync_ShouldSendMessageToKafka()
    {
        // Arrange
        var topic = "test-topic";
        var message = new { Id = Guid.NewGuid(), Name = "Test Message" };

        // Act
        await _transactionProducer.SendAsync(topic, message);

        // Assert
        _mockProducer.Verify(x => x.ProduceAsync(
            topic,
            It.Is<Message<string, string>>(m => m.Value == JsonConvert.SerializeObject(message)),
            default), Times.Once);
    }

    [Fact]
    public async Task PublishTransactionCreated_ShouldSendTransactionCreatedEvent()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = TransferType.Internal,
            Value = 100.00m,
            CreatedAt = DateTime.UtcNow,
            Status = TransactionStatus.Pending
        };

        // Act
        await _transactionProducer.PublishTransactionCreated(transaction);

        // Assert
        _mockProducer.Verify(x => x.ProduceAsync(
            "transaction-created",
            It.Is<Message<string, string>>(m => m.Value.Contains(transaction.Id.ToString())),
            default), Times.Once);
    }
}
