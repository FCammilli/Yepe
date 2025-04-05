using antifraud.domain.Enums;
using transaction_service.application.DTOs;
using transaction_service.domain.Entities;

namespace transaction_service.application.Services;

public interface ITransactionService
{
    Task<string> Create(CreateTransactionDTO createTransactionDTO);
    Task<Transaction> Get(Guid transaction, DateOnly createdAt);
    Task UpdateStatus(Guid transaction, TransactionStatus transactionStatus);
}