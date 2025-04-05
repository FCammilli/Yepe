using antifraud.domain.Enums;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SharedKernel.Enums;
using SharedKernel.Events;
using transaction_service.application.Services;

namespace transaction_service.infrastructure.Kafka;

public class TransactionUpdateConsumer : ITransactionUpdateConsumer
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionUpdateConsumer> _logger;
    public TransactionUpdateConsumer(ITransactionService antiFraudService, IOptions<KafkaSettings> kafkaSettings, ILogger<TransactionUpdateConsumer> logger)
    {
        var config = new ConsumerConfig
        {
            GroupId = kafkaSettings.Value.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            BootstrapServers = kafkaSettings.Value.BootstrapServers,
        };

        _logger = logger;
        _transactionService = antiFraudService;
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    public async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe("transaction-updated");
        _logger.LogInformation("Kafka Consumer is listening on the topic 'transaction-updated'...");
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(3));
                    if (consumeResult == null)
                    {
                        continue;
                    }

                    var message = consumeResult.Message.Value;
                    var transactionEvent = JsonConvert.DeserializeObject<TransactionStatusUpdateEvent>(message);
                    _logger.LogInformation($"Event received: {message}");

                    if (transactionEvent == null)
                    {
                        _logger.LogWarning("Received null transaction event.");
                        continue;
                    }

                    var status = transactionEvent.Status == TransactionStatusEvent.Approved ? TransactionStatus.Approved : TransactionStatus.Rejected;
                    await _transactionService.UpdateStatus(transactionEvent.TransactionId, status);
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
