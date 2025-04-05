using antifraud_service.application.Repositories;
using MongoDB.Driver;

namespace antifraud_service.infrastructure.Mongo;

public class DailyTransactionSummaryRepository : IDailyTransactionSummaryRepository
{
    private readonly IMongoCollection<domain.Entities.DailyTransactionSummary> _collection;
    public DailyTransactionSummaryRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<domain.Entities.DailyTransactionSummary>("daily_transaction_summary");
    }

    public async Task<decimal> GetTotalForAsync(Guid sourceAccountId, DateTime date)
    {
        var filter = Builders<domain.Entities.DailyTransactionSummary>.Filter.And(
            Builders<domain.Entities.DailyTransactionSummary>.Filter.Eq(x => x.SourceAccountId, sourceAccountId),
            Builders<domain.Entities.DailyTransactionSummary>.Filter.Eq(x => x.Date, date.Date)
        );
        var summary = await _collection.Find(filter).FirstOrDefaultAsync();
        return summary?.TotalApprovedValue ?? 0;
    }

    public async Task AddToTotalAsync(Guid sourceAccountId, DateTime date, decimal value)
    {
        var id = $"{sourceAccountId}-{date:yyyyMMdd}";
        var filter = Builders<domain.Entities.DailyTransactionSummary>.Filter.And(
            Builders<domain.Entities.DailyTransactionSummary>.Filter.Eq(x => x.Id, id),
            Builders<domain.Entities.DailyTransactionSummary>.Filter.Eq(x => x.SourceAccountId, sourceAccountId),
            Builders<domain.Entities.DailyTransactionSummary>.Filter.Eq(x => x.Date, date.Date)
        );
        var update = Builders<domain.Entities.DailyTransactionSummary>.Update.Inc(x => x.TotalApprovedValue, value);
        await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }
}
