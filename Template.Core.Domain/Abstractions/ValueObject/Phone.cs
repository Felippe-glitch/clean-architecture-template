using System;
using System.Text.RegularExpressions;
using Template.Core.CrossCutting.Exceptions;

namespace Template.Core.Domain.Abstractions.ValueObject;

public record Phone
{
    public string AreaCode { get; init; }
    public string Formatted { get; init; }
    public string DigitsOnly { get; init; }

    public Phone(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Phone cannot be empty.");
        }

        DigitsOnly = Regex.Replace(value, @"\D", "");

        if (DigitsOnly.Length is < 10 or > 11)
        {
            throw new BusinessRuleException("Phone must contain 10 or 11 digits, including the area code.");
        }

        AreaCode = DigitsOnly.Substring(0, 2);

        if (DigitsOnly.Length == 11)
        {
            Formatted = Regex.Replace(DigitsOnly, @"(\d{2})(\d{5})(\d{4})", "($1) $2-$3");
        }
        else
        {
            Formatted = Regex.Replace(DigitsOnly, @"(\d{2})(\d{4})(\d{4})", "($1) $2-$3");
        }
    }
}
