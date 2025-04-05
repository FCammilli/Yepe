using transaction_service.domain.Entities;

namespace transaction_service.application.Services
{
    public interface ITransactionProducer
    {
        Task PublishTransactionCreated(Transaction transaction);
        Task SendAsync<T>(string topic, T message);
    }
}