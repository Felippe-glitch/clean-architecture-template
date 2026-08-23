using FluentNHibernate.Mapping;
using Template.Core.Domain.Usuarios.Entity;

namespace Template.Core.Infra.Usuarios.Mappers;

public class UsuarioClassMap : ClassMap<Usuario>
{
    public UsuarioClassMap()
    {
        Not.LazyLoad();

        Table("usuario");
        Schema("public");

        Id(x => x.Id)
            .Column("id")
            .GeneratedBy.Identity();

        Map(x => x.Login)
            .Column("login");

        Map(x => x.SenhaHash)
            .Column("senha_hash");

        Map(x => x.Email)
            .Column("email");

        Map(x => x.Role)
            .Column("role")
            .CustomType<int>();

        Map(x => x.Ativo)
            .Column("ativo");
    }
}
