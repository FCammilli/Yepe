using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace antifraud_service.application.Services;

public class TransactionApprovedEventHandler : INotificationHandler<TransactionApprovedEvent>
{
    private readonly IDailyTransactionAccumulator _accumulator;
    public TransactionApprovedEventHandler(IDailyTransactionAccumulator accumulator)
    {
        _accumulator = accumulator;
    }
    public async Task Handle(TransactionApprovedEvent notification, CancellationToken cancellationToken)
    {
        var accumulatedValue = await _accumulator.GetAccumulatedValueAsync(notification.Date);
        if (accumulatedValue + notification.Value > 20000)
        {
            // Lógica para rechazar transacción
            Console.WriteLine("Transacción rechazada debido al acumulado diario");
        }
    }
}