using antifraud_service.application.Services;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SharedKernel.Events;

namespace antifraud_service.infrastructure.Kafka;

public class TransactionCreatedConsumer : IAntifraudTransactionCreatedConsumer
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IAntiFraudService _antiFraudService;
    private readonly ILogger<TransactionCreatedConsumer> _logger;
    public TransactionCreatedConsumer(IAntiFraudService antiFraudService, IOptions<KafkaSettings> kafkaSettings, ILogger<TransactionCreatedConsumer> logger)
    {
        var config = new ConsumerConfig
        {
            GroupId = kafkaSettings.Value.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            BootstrapServers = kafkaSettings.Value.BootstrapServers,
        };

        _logger = logger;
        _antiFraudService = antiFraudService;
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    public async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe("transaction-created");
        _logger.LogInformation("Kafka Consumer is listening on the topic 'transaction-created'...");
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    var message = consumeResult.Message.Value;

                    if (message == null)
                    {
                        _logger.LogWarning("Received null transaction event.");
                        continue;
                    }

                    var transactionEvent = JsonConvert.DeserializeObject<TransactionCreatedEvent>(message);
                    _logger.LogInformation($"Event received: {message}");

                    if (transactionEvent == null)
                    {
                        _logger.LogWarning("Null conversion to transaction event.");
                        continue;
                    }

                    await _antiFraudService.ProcessTransactionAsync(transactionEvent);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing message: {ex.Message}");
                }
            }
        }
        finally
        {
            _consumer.Close();
        }
    }
}
