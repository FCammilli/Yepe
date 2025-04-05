using antifraud.domain.Enums;

namespace transaction_service.application.DTOs
{
    public class CreateTransactionDTO
    {
        public required Guid SourceAccountId { get; set; }
        public required Guid TargetAccountId { get; set; }
        public required TransferType TransferTypeId { get; set; }

        public required decimal Value { get; set; }
    }
}
