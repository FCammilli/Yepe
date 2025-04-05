using antifraud.domain.Enums;

namespace antifraud.domain.Entities;

public interface ITransaction
{
    public Guid Id { get; set; }
    public Guid SourceAccountId { get; set; }
    public Guid TargetAccountId { get; set; }
    public TransferType TransferTypeId { get; set; }
    public decimal Value { get; set; }
}
