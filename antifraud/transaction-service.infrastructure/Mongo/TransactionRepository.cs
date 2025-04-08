using antifraud.domain.Enums;
using MongoDB.Driver;
using transaction_service.application.Services;
using transaction_service.domain.Entities;

namespace transaction_service.infrastructure.Mongo;

public class TransactionRepository : ITransactionRepository
{
    private readonly IMongoCollection<Transaction> _collection;
    public TransactionRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Transaction>("transactions");
    }

    public async Task<Transaction> Create(Transaction transaction)
    {
        await _collection.InsertOneAsync(transaction);
        return transaction;
    }

    public async Task UpdateStatus(Guid transactionId, TransactionStatus transactionStatus)
    {
        var filter = Builders<Transaction>.Filter.And(
            Builders<Transaction>.Filter.Eq(x => x.Id, transactionId)
        );
        var update = Builders<Transaction>.Update.Set(x => x.Status, transactionStatus);
        await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task<Transaction> Get(Guid transactionId, DateOnly createdAt)
    {
        var startOfDay = createdAt.ToDateTime(new TimeOnly(0, 0, 0));
        var endOfDay = createdAt.ToDateTime(new TimeOnly(23, 59, 59, 999));

        var filter = Builders<Transaction>.Filter.And(
          Builders<Transaction>.Filter.Eq(x => x.Id, transactionId),
          Builders<Transaction>.Filter.Gte(x => x.CreatedAt, startOfDay),
          Builders<Transaction>.Filter.Lte(x => x.CreatedAt, endOfDay)
      );
        var result = await _collection.FindAsync(filter);

        return result.SingleOrDefault();
    }
}
