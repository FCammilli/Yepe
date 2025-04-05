using antifraud.domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace transaction_service.application.DTOs
{
    public class CreateTransactionDTO
    {
        public required Guid SourceAccountId { get; set; }
        public required Guid TargetAccountId { get; set; }
        public required TransferType TransferTypeId { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "The value must be greater than or equal to 1.")]
        public required decimal Value { get; set; }
    }
}
