using antifraud_service.infrastructure.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using SharedKernel.Enums;
namespace antifraud_service.test.infraestructura.kafka;
public class TransactionAntiFraudProducerTests
{
    private readonly Mock<IOptions<KafkaSettings>> _mockKafkaSettings;
    private readonly Mock<IProducer<string, string>> _mockProducer;
    private readonly TransactionAntiFraudProducer _producer;

    public TransactionAntiFraudProducerTests()
    {
        _mockKafkaSettings = new Mock<IOptions<KafkaSettings>>();
        _mockKafkaSettings.Setup(x => x.Value).Returns(new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group"
        });

        _mockProducer = new Mock<IProducer<string, string>>();
        _producer = new TransactionAntiFraudProducer(_mockKafkaSettings.Object);

        var producerField = typeof(TransactionAntiFraudProducer)
            .GetField("_producer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        producerField.SetValue(_producer, _mockProducer.Object);
    }

    [Fact]
    public async Task SendAsync_ShouldSerializeMessageAndSendToKafka()
    {
        // Arrange
        var topic = "test-topic";
        var message = new { Id = Guid.NewGuid(), Name = "Test" };
        var serializedMessage = JsonConvert.SerializeObject(message);

        // Act
        await _producer.SendAsync(topic, message);

        // Assert
        _mockProducer.Verify(p => p.ProduceAsync(
            topic,
            It.Is<Message<string, string>>(m => m.Value == serializedMessage),
            default), Times.Once);
    }

    [Fact]
    public async Task PublishStatusRejectedAsync_ShouldSendRejectedEventToKafka()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var value = 100m;

        // Act
        await _producer.PublishStatusRejectedAsync(transactionId, value);

        // Assert
        _mockProducer.Verify(p => p.ProduceAsync(
            "transaction-updated",
            It.Is<Message<string, string>>(m =>
                m.Value.Contains($"\"TransactionId\":\"{transactionId}\"") &&
                m.Value.Contains($"\"Value\":{value}") &&
                m.Value.Contains($"\"Status\":{(int)TransactionStatusEvent.Rejected}")),
            default), Times.Once);
    }

    [Fact]
    public async Task PublishStatusApprovedAsync_ShouldSendApprovedEventToKafka()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var value = 200m;

        // Act
        await _producer.PublishStatusApprovedAsync(transactionId, value);

        // Assert
        _mockProducer.Verify(p => p.ProduceAsync(
            "transaction-updated",
            It.Is<Message<string, string>>(m =>
                m.Value.Contains($"\"TransactionId\":\"{transactionId}\"") &&
                m.Value.Contains($"\"Value\":{value}") &&
                m.Value.Contains($"\"Status\":{(int)TransactionStatusEvent.Approved}")),
            default), Times.Once);
    }
}
