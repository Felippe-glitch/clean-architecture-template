using System;
using System.Text.RegularExpressions;

namespace Template.Core.Domain.Abstractions.ValueObject;

public record Endereco
{
    public string Rua { get; init; }
    public int Numero { get; init; }
    public string Bairro { get; init; }
    public string Cidade { get; init; }
    public string UF { get; init; }
    public string CEP { get; init; }
    public string CepFormatado { get; init; }

    // Construtor 1: Recebendo uma string única
    public Endereco(string _endereco)
    {
        if (string.IsNullOrWhiteSpace(_endereco))
        {
            throw new ArgumentException("O endereço não pode estar vazio.");
        }

        string[] partes = _endereco.Split(',');

        if (partes.Length != 6)
        {
            // AJUSTE 1: Mensagem de erro atualizada para incluir o "Número"
            throw new ArgumentException("Formato de string inválido. Use o padrão: 'Rua, Número, Bairro, Cidade, UF, CEP'.");
        }

        Rua = partes[0].Trim();
        
        // AJUSTE 2: Forma segura de converter string para int
        if (!int.TryParse(partes[1].Trim(), out int numeroConvertido))
        {
            throw new ArgumentException("O campo 'Número' deve ser um valor numérico válido.");
        }
        Numero = numeroConvertido;

        Bairro = partes[2].Trim();
        Cidade = partes[3].Trim();
        UF = partes[4].Trim().ToUpperInvariant();
        
        CEP = Regex.Replace(partes[5], @"\D", "");

        ValidarCampos();
        CepFormatado = Regex.Replace(CEP, @"(\d{5})(\d{3})", "$1-$2");
    }

    // Construtor 2: Recebendo os dados separados
    public Endereco(string rua, int numero, string bairro, string cidade, string uf, string cep)
    {
        Rua = rua.Trim();
        Numero = numero;
        Bairro = bairro.Trim();
        Cidade = cidade.Trim();
        UF = uf.Trim().ToUpperInvariant();
        CEP = Regex.Replace(cep, @"\D", "");

        ValidarCampos();
        CepFormatado = Regex.Replace(CEP, @"(\d{5})(\d{3})", "$1-$2");
    }

    private void ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(Rua) || string.IsNullOrWhiteSpace(Bairro) || string.IsNullOrWhiteSpace(Cidade))
        {
            throw new ArgumentException("Rua, Bairro e Cidade são obrigatórios.");
        }

        if (UF.Length != 2)
        {
            throw new ArgumentException("A UF (Estado) deve conter exatamente 2 letras.");
        }

        if (CEP.Length != 8)
        {
            throw new ArgumentException("O CEP deve conter exatamente 8 dígitos numéricos.");
        }

        // Bônus: Garantir que o número não seja negativo
        if (Numero < 0)
        {
            throw new ArgumentException("O número do endereço não pode ser negativo.");
        }
    }
}