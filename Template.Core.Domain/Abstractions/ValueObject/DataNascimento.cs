using System;
using System.Globalization;
using Template.Core.Domain.Abstractions.Exceptions;

namespace Template.Core.Domain.Abstractions.ValueObject;

public record DataNascimento
{
    public string StringFormatted { get; init; }
    
    // Agora usando DateOnly no lugar de DateTime
    public DateOnly DateFormatted { get; init; }

    // 1º Construtor: Recebendo String
    public DataNascimento(string _data)
    {
        if (string.IsNullOrWhiteSpace(_data))
        {
            throw new ArgumentException("A data de nascimento não pode estar vazia.");
        }

        // Usando DateOnly.TryParse
        if (!DateOnly.TryParse(_data, new CultureInfo("pt-BR"), DateTimeStyles.None, out DateOnly dataConvertida))
        {
            throw new ArgumentException("Formato de data inválido. Use o padrão dd/MM/yyyy.");
        }

        ValidarRegrasDeNegocio(dataConvertida);

        DateFormatted = dataConvertida; // Não precisamos mais do .Date aqui!
        StringFormatted = dataConvertida.ToString("dd/MM/yyyy");
    }

    // 2º Construtor: Recebendo DateOnly
    public DataNascimento(DateOnly _data)
    {
        ValidarRegrasDeNegocio(_data);

        DateFormatted = _data;
        StringFormatted = _data.ToString("dd/MM/yyyy");
    }

    private static void ValidarRegrasDeNegocio(DateOnly data)
    {
        DateOnly hoje = DateOnly.FromDateTime(DateTime.Now);

        if (data > hoje)
        {
            throw new RegraDeNegocioVioladaException("A data de nascimento não pode ser no futuro.");
        }

        // if (data.Year == hoje.Day)
        // {
        //     throw new ArgumentException("O ano de nascimento não pode ser o ano atual.");
        // }

        if (data.Year < 1900)
        {
            throw new ArgumentException("Data de nascimento muito antiga e inválida para o sistema.");
        }
    }
}