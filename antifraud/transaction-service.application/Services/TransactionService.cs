
using antifraud.domain.Enums;
using transaction_service.application.DTOs;
using transaction_service.domain.Entities;

namespace transaction_service.application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionProducer _transactionProducer;
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(ITransactionRepository transactionRepository, ITransactionProducer transactionProducer)
    {
        _transactionProducer = transactionProducer;
        _transactionRepository = transactionRepository;
    }

    public async Task<string> Create(CreateTransactionDTO createTransactionDTO)
    {
        Transaction transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Value = createTransactionDTO.Value,
            Status = TransactionStatus.Pending,
            TransferTypeId = createTransactionDTO.TransferTypeId,
            SourceAccountId = createTransactionDTO.SourceAccountId,
            TargetAccountId = createTransactionDTO.TargetAccountId,
        };

        await _transactionRepository.Create(transaction);

        await _transactionProducer.PublishTransactionCreated(transaction);

        return transaction.Id.ToString();
    }

    public async Task UpdateStatus(Guid transaction, TransactionStatus transactionStatus)
    {
        await _transactionRepository.UpdateStatus(transaction, transactionStatus);
    }

    public async Task<Transaction> Get(Guid transaction, DateOnly createdAt)
    {
        return await _transactionRepository.Get(transaction, createdAt);
    }
}
