namespace transaction_service.application.Services;

public interface ITransactionUpdateConsumer
{
    Task StartListeningAsync(CancellationToken cancellationToken);
}