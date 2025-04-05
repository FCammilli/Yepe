using Microsoft.AspNetCore.Mvc;
using transaction_service.application.DTOs;
using transaction_service.application.Services;
using transaction_service.domain.Entities;

namespace transaction_service.api.Controllers;

[ApiController]
[Route("[controller]")]
public class TransactionController : ControllerBase
{
    private readonly ILogger<TransactionController> _logger;
    private readonly ITransactionService _transaccionService;

    public TransactionController(ILogger<TransactionController> logger, ITransactionService transaccionService)
    {
        _logger = logger;
        _transaccionService = transaccionService;
    }

    /// <summary>
    /// Creates a new transaction based on the provided transaction data.
    /// </summary>
    /// <param name="createTransactionDTO">The data transfer object containing the details of the transaction to be created.</param>
    /// <returns>
    /// An ActionResult containing the result of the transaction creation. 
    /// Returns a 201 Created status code with the result if successful, otherwise returns a BadRequest result.
    /// </returns>
    [HttpPost(Name = "Create")]
    public async Task<ActionResult<string>> Transaction([FromBody] CreateTransactionDTO createTransactionDTO)
    {
        var result = await _transaccionService.Create(createTransactionDTO);

        if (result == null)
            return BadRequest();

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Retrieves a transaction based on the provided transaction ID and creation date.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the transaction.</param>
    /// <param name="createdDate">The date the transaction was created. MM-dd-yyyy</param>
    /// <returns>
    /// An ActionResult containing the transaction if found, otherwise a BadRequest or NotFound result.
    /// </returns>
    [HttpGet(Name = "Get")]
    public async Task<ActionResult<Transaction>> Transaction([FromQuery] Guid transactionId, DateOnly? createdDate)
    {
        if (transactionId == Guid.Empty && createdDate == null)
        {
            return BadRequest(new { error = "The date and id parameters are required." });
        }

        var result = await _transaccionService.Get(transactionId, createdDate.Value);

        if (result == null)
            return NotFound();

        return StatusCode(StatusCodes.Status200OK, result);
    }
}
