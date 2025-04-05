namespace antifraud_service.application.Services;
public interface IAntifraudTransactionCreatedConsumer
{
    Task StartListeningAsync(CancellationToken cancellationToken);
}