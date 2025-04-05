using antifraud.domain.Entities;

namespace antifraud_service.domain.Entities;

public class DailyTransactionSummary : IDailyTransactionSummary
{
    public required string Id { get; set; }
    public DateTime Date { get; set; }
    public Guid SourceAccountId { get; set; }
    public decimal TotalApprovedValue { get; set; }
}
