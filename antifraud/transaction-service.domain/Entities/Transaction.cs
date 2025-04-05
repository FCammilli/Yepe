using antifraud.domain.Entities;
using antifraud.domain.Enums;

namespace transaction_service.domain.Entities;

public class Transaction : ITransaction
{
    public required Guid Id { get; set; }
    public required Guid SourceAccountId { get; set; }
    public required Guid TargetAccountId { get; set; }
    public required TransferType TransferTypeId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required TransactionStatus Status { get; set; }
    public required decimal Value { get; set; }
}
