using Library.Api.Common.Results;
using Library.Api.Common.Responses;
using Library.Application.Handlers;
using Library.Application.Models.Views;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

/// <summary>
/// Reads loans and returns active loans.
/// </summary>
[ApiController]
[Route("api/loans")]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(ApiProblemDetails))]
public sealed class LoanController(GetLoanHandler getLoanHandler,
    ListLoansHandler listLoansHandler,
    ReturnLoanHandler returnLoanHandler) : ControllerBase
{
    #region Public Methods

    /// <summary>
    /// Lists loans from the MongoDB read model.
    /// </summary>
    /// <remarks>
    /// The read model is eventually consistent. Recently created or returned loans can take a short time to appear.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LoanView>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await listLoansHandler.HandleAsync(cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets one loan from the MongoDB read model.
    /// </summary>
    [HttpGet("{loanId:guid}")]
    [ProducesResponseType<LoanView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid loanId, CancellationToken cancellationToken)
    {
        var result = await getLoanHandler.HandleAsync(loanId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Returns an active loan.
    /// </summary>
    /// <remarks>
    /// Returning an already returned loan produces 409. Concurrent return attempts are protected by the loan version token.
    /// </remarks>
    [HttpPost("{loanId:guid}/return")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Return(Guid loanId, CancellationToken cancellationToken)
    {
        var result = await returnLoanHandler.HandleAsync(new(loanId), cancellationToken);
        return result.ToActionResult();
    }

    #endregion
}
