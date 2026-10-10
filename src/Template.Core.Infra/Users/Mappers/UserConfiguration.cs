using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Template.Core.Domain.Users.Entity;

namespace Template.Core.Infra.Users.Mappers;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Login)
            .HasColumnName("login");

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash");

        builder.Property(u => u.Email)
            .HasColumnName("email");

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasConversion<int>();

        builder.Property(u => u.Active)
            .HasColumnName("active");
    }
}
