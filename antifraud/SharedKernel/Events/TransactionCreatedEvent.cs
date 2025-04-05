namespace SharedKernel.Events;

public record TransactionCreatedEvent(
Guid TransactionId,
Guid SourceAccountId,
decimal Value,
DateTime CreatedAt
);
