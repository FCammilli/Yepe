using antifraud.domain.Enums;
using transaction_service.domain.Entities;

namespace transaction_service.application.Services;

public interface ITransactionRepository
{

    Task<Transaction> Create(Transaction transaction);
    Task<Transaction> Get(Guid transactionId, DateOnly createdAt);
    Task UpdateStatus(Guid transactionId, TransactionStatus transactionStatus);
}