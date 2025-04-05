using antifraud_service.application.Services;

namespace antifraud_service.worker;

public class AntiFraudWorker : BackgroundService
{
    private readonly IAntifraudTransactionCreatedConsumer _consumer;

    public AntiFraudWorker(IAntifraudTransactionCreatedConsumer consumer)
    {
        _consumer = consumer;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.StartListeningAsync(stoppingToken);
    }
}
