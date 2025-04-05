using SharedKernel.Enums;

namespace SharedKernel.Events;

public record TransactionStatusUpdateEvent(
    Guid TransactionId,
    Decimal Value,
    DateTime ProcessedAt,
    TransactionStatusEvent Status
);

