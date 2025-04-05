namespace antifraud_service.application.Repositories;

public interface IDailyTransactionSummaryRepository
{
    Task<decimal> GetTotalForAsync(Guid sourceAccountId, DateTime date);
    Task AddToTotalAsync(Guid sourceAccountId, DateTime date, decimal value);
}