using System.Text.RegularExpressions;
using Template.Core.Domain.Abstractions.Exceptions;

namespace Template.Core.Domain.Abstractions.ValueObject;

public record CPF
{
    public string Formatado {get; init;}
    public string SomenteNumeros {get; init;}

    public CPF(string _cpf)
    {
        if (string.IsNullOrWhiteSpace(_cpf))
        {
            throw new ArgumentException("CPF não pode estar vazio");
        }

        string padraoValidacao = @"^\d{3}[-.]?\d{3}[-.]?\d{3}[-.]?\d{2}$";

        if (!Regex.IsMatch(_cpf, padraoValidacao))
        {
            throw new RegraDeNegocioVioladaException("CPF não tem onze digitos");
        }       

        string numerosExtraidos = Regex.Replace(_cpf, @"\D", "");

        if (!Validar(numerosExtraidos))
        {
            throw new RegraDeNegocioVioladaException("O CPF informado é matematicamente inválido (inventado).");
        }

        // 3. Se passou pela validação, preenche as propriedades do Record
        SomenteNumeros = numerosExtraidos;
        Formatado = Regex.Replace(SomenteNumeros, @"(\d{3})(\d{3})(\d{3})(\d{2})", "$1.$2.$3-$4");
    }

    private bool Validar(string cpf)
    {
        if (cpf.All(c => c == cpf[0]))
            return false;

        int[] pesosPrimeiroDigito = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int soma = 0;

        for (int i = 0; i < 9; i++)
        {
            // Convertemos o caractere char numérico para int de forma eficiente
            int digito = cpf[i] - '0'; 
            soma += digito * pesosPrimeiroDigito[i];
        }

        int resto = soma % 11;
        int primeiroDigitoVerificador = resto < 2 ? 0 : 11 - resto;

        // Se o primeiro dígito calculado não bater com o 10º dígito do CPF digitado, é inválido
        if (primeiroDigitoVerificador != (cpf[9] - '0'))
            return false;

        // --- CÁLCULO DO 2º DÍGITO VERIFICADOR ---
        int[] pesosSegundoDigito = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];
        soma = 0;

        for (int i = 0; i < 10; i++)
        {
            int digito = cpf[i] - '0';
            soma += digito * pesosSegundoDigito[i];
        }

        resto = soma % 11;
        int segundoDigitoVerificador = resto < 2 ? 0 : 11 - resto;

        // Se o segundo dígito calculado bater com o 11º dígito do CPF, ele é legítimo!
        return segundoDigitoVerificador == (cpf[10] - '0');
    }
}