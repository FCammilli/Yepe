namespace antifraud.domain.Entities;

public interface IDailyTransactionSummary
{
    public string Id { get; set; }
    public Guid SourceAccountId { get; set; }
    public DateTime Date { get; set; }
    public decimal TotalApprovedValue { get; set; }
}
