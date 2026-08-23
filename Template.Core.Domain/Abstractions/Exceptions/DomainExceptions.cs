namespace Template.Core.Domain.Abstractions.Exceptions
{

    public class EntidadeNaoEncontradaException : Exception
    {
        public EntidadeNaoEncontradaException(string entidade) : base($"entidade {entidade} não encontrada"){}
        public EntidadeNaoEncontradaException(Type entidade) : base($"entidade {entidade.Name} não encontrada"){}
        public EntidadeNaoEncontradaException(string entidade, string id) : base($"entidade {entidade} de id: {id} não encontrada"){}
    }
    public class EntidadeDesativadaException : Exception
    {
        public EntidadeDesativadaException(string entidade, string id) : base($"entidade {entidade} de id: {id} esta desativada"){}
    }
    public class RegraDeNegocioVioladaException(string regra) : Exception($"{regra}"){}
}