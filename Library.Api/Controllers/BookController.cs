using Library.Api.Requests.Book;
using Library.Api.Common.Results;
using Library.Api.Common.Responses;
using Microsoft.AspNetCore.Mvc;
using Library.Application.Handlers;
using Library.Application.Models.Results;
using Library.Application.Models.Views;

namespace Library.Api.Controllers;

/// <summary>
/// Manages books and starts loans from available book copies.
/// </summary>
[ApiController]
[Route("api/books")]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(ApiProblemDetails))]
public sealed class BookController(
    CreateBookHandler createBookHandler,
    CreateLoanHandler createLoanHandler,
    GetBookHandler getBookHandler,
    ListBooksHandler listBooksHandler) : ControllerBase
{
    #region Public Methods

    /// <summary>
    /// Creates a book in the authoritative MySQL write model.
    /// </summary>
    /// <remarks>
    /// Use the returned id to create a loan through POST /api/books/{bookId}/loans.
    /// </remarks>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<BookResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookRequest request, CancellationToken cancellationToken)
    {
        var result = await createBookHandler.HandleAsync(new(
            request.Title,
            request.Author,
            request.PublicationYear,
            request.AvailableQuantity), cancellationToken);

        return result.ToCreatedAtActionResult(
            nameof(Get),
            value => new { bookId = value.Id },
            value => value);
    }

    /// <summary>
    /// Lists books from the MongoDB read model.
    /// </summary>
    /// <remarks>
    /// The read model is eventually consistent. A recently created book can take a short time to appear.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<BookView>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await listBooksHandler.HandleAsync(cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets one book from the MongoDB read model.
    /// </summary>
    [HttpGet("{bookId:guid}")]
    [ProducesResponseType<BookView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid bookId, CancellationToken cancellationToken)
    {
        var result = await getBookHandler.HandleAsync(bookId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Creates a loan for an available copy of the book.
    /// </summary>
    /// <remarks>
    /// Availability is checked against MySQL, not the read model. Concurrent attempts for the last copy return 409 for the losing request.
    /// </remarks>
    [HttpPost("{bookId:guid}/loans")]
    [ProducesResponseType<LoanResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateLoan(Guid bookId, CancellationToken cancellationToken)
    {
        var result = await createLoanHandler.HandleAsync(new(bookId), cancellationToken);

        return result.ToCreatedAtActionResult(
            nameof(LoanController.Get),
            "Loan",
            value => new { loanId = value.Id },
            value => value);
    }

    #endregion
}
