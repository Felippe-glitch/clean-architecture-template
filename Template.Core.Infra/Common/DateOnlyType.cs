using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Template.Core.Infra.Common;

/// <summary>
/// Mapeia <see cref="DateOnly"/> para uma coluna DATE. O NHibernate 5.4 não
/// possui tipo nativo para <see cref="DateOnly"/>, então sem este user type
/// a propriedade é tratada como serializável e falha ao ler ("Could not
/// deserialize a serializable property").
/// </summary>
public class DateOnlyType : IUserType
{
    public SqlType[] SqlTypes => new[] { NHibernateUtil.Date.SqlType };

    public Type ReturnedType => typeof(DateOnly);

    public bool IsMutable => false;

    public new bool Equals(object x, object y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Equals(y);
    }

    public int GetHashCode(object x) => x?.GetHashCode() ?? 0;

    public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        int ordinal = rs.GetOrdinal(names[0]);
        if (rs.IsDBNull(ordinal)) return null;

        // O Npgsql retorna colunas DATE como System.DateOnly; DateTime é aceito
        // como fallback caso o provedor mude o comportamento.
        object value = rs.GetValue(ordinal);
        return value switch
        {
            DateOnly dateOnly => dateOnly,
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            _ => DateOnly.Parse(value.ToString()!),
        };
    }

    public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
    {
        // O Npgsql aceita System.DateOnly diretamente para parâmetros de coluna DATE.
        cmd.Parameters[index].Value = value is DateOnly dateOnly ? dateOnly : DBNull.Value;
    }

    public object DeepCopy(object value) => value;

    public object Replace(object original, object target, object owner) => original;

    public object Assemble(object cached, object owner) => cached;

    public object Disassemble(object value) => value;
}
