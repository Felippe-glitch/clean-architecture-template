using System.ComponentModel.DataAnnotations;
using Template.Core.Domain.Abstractions;

namespace Template.Core.App.Common;

public class PaginatedRequest<T>
{
    // Quantidade de registros por página
    [Range(1, 200, ErrorMessage = "qt deve estar entre 1 e 200.")]
    public int Qt { get; set; }

    // Número da página
    [Range(1, int.MaxValue, ErrorMessage = "page deve ser maior ou igual a 1.")]
    public int Page { get; set; }

    // Campo utilizado para ordenação
    public string CpOrd { get; set; }

    // Tipo de ordenação
    public TipoOrdenacao TpOrd { get; set; }

    public PaginatedRequest(
        int qt = 10,
        int page = 1,
        string cpOrd = "Id",
        TipoOrdenacao tpOrd = TipoOrdenacao.Ascendente)
    {
        Qt = qt;
        Page = page;
        CpOrd = cpOrd;
        TpOrd = tpOrd;
    }
}