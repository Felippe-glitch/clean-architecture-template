using System.Text.RegularExpressions;
using Template.Core.CrossCutting.Exceptions;

namespace Template.Core.Domain.Abstractions.ValueObject;

/// <summary>CPF: the Brazilian individual taxpayer registry number.</summary>
public record CPF
{
    public string Formatted {get; init;}
    public string DigitsOnly {get; init;}

    public CPF(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("CPF cannot be empty");
        }

        string validationPattern = @"^\d{3}[-.]?\d{3}[-.]?\d{3}[-.]?\d{2}$";

        if (!Regex.IsMatch(value, validationPattern))
        {
            throw new BusinessRuleException("CPF does not have eleven digits");
        }

        string extractedDigits = Regex.Replace(value, @"\D", "");

        if (!IsValid(extractedDigits))
        {
            throw new BusinessRuleException("The provided CPF is mathematically invalid (made up).");
        }

        // 3. Once validation passes, populate the record's properties
        DigitsOnly = extractedDigits;
        Formatted = Regex.Replace(DigitsOnly, @"(\d{3})(\d{3})(\d{3})(\d{2})", "$1.$2.$3-$4");
    }

    private bool IsValid(string cpf)
    {
        if (cpf.All(c => c == cpf[0]))
            return false;

        int[] firstDigitWeights = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int sum = 0;

        for (int i = 0; i < 9; i++)
        {
            // Convert the numeric char to int efficiently
            int digit = cpf[i] - '0';
            sum += digit * firstDigitWeights[i];
        }

        int remainder = sum % 11;
        int firstCheckDigit = remainder < 2 ? 0 : 11 - remainder;

        // If the computed first digit doesn't match the CPF's 10th digit, it's invalid
        if (firstCheckDigit != (cpf[9] - '0'))
            return false;

        // --- SECOND CHECK DIGIT CALCULATION ---
        int[] secondDigitWeights = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];
        sum = 0;

        for (int i = 0; i < 10; i++)
        {
            int digit = cpf[i] - '0';
            sum += digit * secondDigitWeights[i];
        }

        remainder = sum % 11;
        int secondCheckDigit = remainder < 2 ? 0 : 11 - remainder;

        // If the computed second digit matches the CPF's 11th digit, it's legitimate!
        return secondCheckDigit == (cpf[10] - '0');
    }
}
