using System;
using System.Text.RegularExpressions;
using Template.Core.Domain.Abstractions.Exceptions;

namespace Template.Core.Domain.Abstractions.ValueObject;

public record Telefone
{
    public string DDD { get; init; }
    public string Formatado { get; init; }
    public string SomenteNumeros { get; init; } // Útil para salvar no banco

    public Telefone(string _telefone)
    {
        if (string.IsNullOrWhiteSpace(_telefone))
        {
            throw new ArgumentException("O telefone não pode estar vazio.");
        }

        // Remove tudo que não for número
        SomenteNumeros = Regex.Replace(_telefone, @"\D", "");

        // Valida a quantidade de dígitos (10 para fixo, 11 para celular com o 9 à frente)
        if (SomenteNumeros.Length is < 10 or > 11)
        {
            throw new RegraDeNegocioVioladaException("O telefone deve conter 10 ou 11 dígitos, incluindo o DDD.");
        }

        // O DDD sempre serão os dois primeiros números
        DDD = SomenteNumeros.Substring(0, 2);

        // Aplica a máscara dependendo do tamanho (Fixo vs Celular)
        if (SomenteNumeros.Length == 11)
        {
            Formatado = Regex.Replace(SomenteNumeros, @"(\d{2})(\d{5})(\d{4})", "($1) $2-$3");
        }
        else
        {
            Formatado = Regex.Replace(SomenteNumeros, @"(\d{2})(\d{4})(\d{4})", "($1) $2-$3");
        }
    }
}