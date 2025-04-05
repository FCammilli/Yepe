using antifraud_service.application.Repositories;
using SharedKernel.Events;

namespace antifraud_service.application.Services;

public class AntiFraudService : IAntiFraudService
{
    private readonly IAntifraudTransactionProducer _producer;
    private readonly IDailyTransactionSummaryRepository _accumulator;

    public AntiFraudService(IDailyTransactionSummaryRepository accumulator, IAntifraudTransactionProducer producer)
    {
        _producer = producer;
        _accumulator = accumulator;
    }
    public async Task ProcessTransactionAsync(TransactionCreatedEvent transaction)
    {
        var today = DateTime.UtcNow.Date;
        var transactionId = transaction.TransactionId;
        var transactionValue = transaction.Value;
        var sourceAccountId = transaction.SourceAccountId;

        if (transactionValue > 2000)
        {
            await _producer.PublishStatusRejectedAsync(transactionId, transactionValue);
            return;
        }

        var accumulated = await _accumulator.GetTotalForAsync(sourceAccountId, today);

        if (accumulated + transactionValue > 2000)
        {
            await _producer.PublishStatusRejectedAsync(transactionId, transactionValue);
            return;
        }

        await _accumulator.AddToTotalAsync(sourceAccountId, today, transactionValue);
        await _producer.PublishStatusApprovedAsync(transactionId, transactionValue);

    }
}