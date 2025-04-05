namespace antifraud_service.application.Services;

public interface IAntifraudTransactionProducer
{
    Task SendAsync<T>(string topic, T message);
    Task PublishStatusRejectedAsync(Guid transactionId, decimal value);
    Task PublishStatusApprovedAsync(Guid transactionId, decimal value);
}
