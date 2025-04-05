using SharedKernel.Events;

namespace antifraud_service.application.Services;

public interface IAntiFraudService
{
    Task ProcessTransactionAsync(TransactionCreatedEvent transactionEvent);
}
