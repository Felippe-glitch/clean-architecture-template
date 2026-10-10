using System;
using System.Text.RegularExpressions;

namespace Template.Core.Domain.Abstractions.ValueObject;

public record Address
{
    public string Street { get; init; }
    public int Number { get; init; }
    public string Neighborhood { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string ZipCode { get; init; }
    public string FormattedZipCode { get; init; }

    public Address(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Address cannot be empty.");
        }

        string[] parts = value.Split(',');

        if (parts.Length != 6)
        {
            throw new ArgumentException("Invalid string format. Use the pattern: 'Street, Number, Neighborhood, City, State, ZipCode'.");
        }

        Street = parts[0].Trim();

        if (!int.TryParse(parts[1].Trim(), out int parsedNumber))
        {
            throw new ArgumentException("The 'Number' field must be a valid numeric value.");
        }
        Number = parsedNumber;

        Neighborhood = parts[2].Trim();
        City = parts[3].Trim();
        State = parts[4].Trim().ToUpperInvariant();

        ZipCode = Regex.Replace(parts[5], @"\D", "");

        ValidateFields();
        FormattedZipCode = Regex.Replace(ZipCode, @"(\d{5})(\d{3})", "$1-$2");
    }

    public Address(string street, int number, string neighborhood, string city, string state, string zipCode)
    {
        Street = street.Trim();
        Number = number;
        Neighborhood = neighborhood.Trim();
        City = city.Trim();
        State = state.Trim().ToUpperInvariant();
        ZipCode = Regex.Replace(zipCode, @"\D", "");

        ValidateFields();
        FormattedZipCode = Regex.Replace(ZipCode, @"(\d{5})(\d{3})", "$1-$2");
    }

    private void ValidateFields()
    {
        if (string.IsNullOrWhiteSpace(Street) || string.IsNullOrWhiteSpace(Neighborhood) || string.IsNullOrWhiteSpace(City))
        {
            throw new ArgumentException("Street, Neighborhood and City are required.");
        }

        if (State.Length != 2)
        {
            throw new ArgumentException("The State must contain exactly 2 letters.");
        }

        if (ZipCode.Length != 8)
        {
            throw new ArgumentException("The ZipCode must contain exactly 8 numeric digits.");
        }

        // Bonus: ensure the number isn't negative
        if (Number < 0)
        {
            throw new ArgumentException("The address number cannot be negative.");
        }
    }
}
