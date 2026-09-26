using GestionTubos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionTubos.Infrastructure.Configurations;

public sealed class DireccionClienteConfiguration : IEntityTypeConfiguration<DireccionCliente>
{
    public void Configure(EntityTypeBuilder<DireccionCliente> builder)
    {
        builder.ToTable("DireccionesCliente");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Calle).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Numero).HasMaxLength(20);
        builder.Property(x => x.PisoDepartamento).HasMaxLength(50);
        builder.Property(x => x.Localidad).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Provincia).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CodigoPostal).HasMaxLength(12);
        builder.HasOne(x => x.Cliente)
            .WithMany(x => x.Direcciones)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => x.Activa);
    }
}
