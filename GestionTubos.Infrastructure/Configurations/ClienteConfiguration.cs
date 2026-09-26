using GestionTubos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionTubos.Infrastructure.Configurations;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(100);
        builder.Property(x => x.Apellido).HasMaxLength(100);
        builder.Property(x => x.RazonSocial).HasMaxLength(200);
        builder.Property(x => x.NombreComercial).HasMaxLength(200);
        builder.Property(x => x.Dni).HasMaxLength(20);
        builder.Property(x => x.CuitCuil).HasMaxLength(20);
        builder.Property(x => x.Responsable).HasMaxLength(150);
        builder.Property(x => x.Telefono).HasMaxLength(50);
        builder.Property(x => x.Correo).HasMaxLength(254);
        builder.HasIndex(x => x.Dni).IsUnique().HasFilter("[Dni] IS NOT NULL");
        builder.HasIndex(x => x.CuitCuil).IsUnique().HasFilter("[CuitCuil] IS NOT NULL");
        builder.HasQueryFilter(x => x.Activo);
    }
}
