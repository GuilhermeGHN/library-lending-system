using Library.Application.Commands;
using Library.Application.Common.Results;

namespace Library.Application.Validators;

public static class CreateBookCommandValidator
{
    #region Public Methods

    public static Result Validate(CreateBookCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(command.Title))
            errors["title"] = ["Title is required."];
        else if (command.Title.Trim().Length > 200)
            errors["title"] = ["Title cannot exceed 200 characters."];
        
        if (string.IsNullOrWhiteSpace(command.Author))
            errors["author"] = ["Author is required."];
        else if (command.Author.Trim().Length > 200)
            errors["author"] = ["Author cannot exceed 200 characters."];
        
        if (command.PublicationYear is < 1 or > 9999)
            errors["publicationYear"] = ["Publication year must be between 1 and 9999."];
        
        if (command.AvailableQuantity < 0)
            errors["availableQuantity"] = ["Available quantity cannot be negative."];
        
        return errors.Count > 0
            ? Result.Fail(Error.Validation(errors))
            : Result.Ok();
    }

    #endregion
}
