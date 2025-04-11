using transaction_service.application.Services;

namespace transaction_service.api;

public class TransactionWorker : BackgroundService
{
    private readonly ITransactionUpdateConsumer _consumer;

    public TransactionWorker(ITransactionUpdateConsumer consumer)
    {
        _consumer = consumer;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(() => _consumer.StartListeningAsync(stoppingToken), stoppingToken);
    }
}
