using BuildingBlocks.Correlation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transactions.Application.Commands.AddTransactionItem;
using Transactions.Application.Commands.CancelTransaction;
using Transactions.Application.Commands.CreateTransaction;
using Transactions.Application.Commands.SubmitTransaction;
using Transactions.Application.DTOs;
using Transactions.Application.Queries.GetAllTransactions;
using Transactions.Application.Queries.GetTransaction;

namespace Transactions.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController(IMediator mediator, ICorrelationContext correlationContext)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddItemRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new AddTransactionItemCommand(id, request.ProductId, request.Quantity, request.Price), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        await mediator.Send(new SubmitTransactionCommand(id, correlationContext.CorrelationId), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await mediator.Send(new CancelTransactionCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTransactionQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllTransactionsQuery(), ct);
        return Ok(result);
    }
}

public record AddItemRequest(string ProductId, int Quantity, decimal Price);
