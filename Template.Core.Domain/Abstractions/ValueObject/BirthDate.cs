using System;
using System.Globalization;
using Template.Core.CrossCutting.Exceptions;

namespace Template.Core.Domain.Abstractions.ValueObject;

public record BirthDate
{
    public string StringFormatted { get; init; }

    // Uses DateOnly instead of DateTime
    public DateOnly DateFormatted { get; init; }

    // 1st constructor: from a string
    public BirthDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Birth date cannot be empty.");
        }

        // Using DateOnly.TryParse
        if (!DateOnly.TryParse(value, new CultureInfo("pt-BR"), DateTimeStyles.None, out DateOnly parsedDate))
        {
            throw new ArgumentException("Invalid date format. Use dd/MM/yyyy.");
        }

        ValidateBusinessRules(parsedDate);

        DateFormatted = parsedDate; // No need for .Date here anymore!
        StringFormatted = parsedDate.ToString("dd/MM/yyyy");
    }

    // 2nd constructor: from a DateOnly
    public BirthDate(DateOnly value)
    {
        ValidateBusinessRules(value);

        DateFormatted = value;
        StringFormatted = value.ToString("dd/MM/yyyy");
    }

    private static void ValidateBusinessRules(DateOnly date)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        if (date > today)
        {
            throw new BusinessRuleException("Birth date cannot be in the future.");
        }

        if (date.Year < 1900)
        {
            throw new ArgumentException("Birth date is too old and invalid for this system.");
        }
    }
}
