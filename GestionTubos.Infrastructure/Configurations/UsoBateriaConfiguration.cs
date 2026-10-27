using GestionTubos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace GestionTubos.Infrastructure.Configurations;
public sealed class UsoBateriaConfiguration:IEntityTypeConfiguration<UsoBateria>
{
 public void Configure(EntityTypeBuilder<UsoBateria>b){b.ToTable("UsosBateria");b.HasKey(x=>x.Id);b.HasIndex(x=>x.TuboId).IsUnique().HasFilter("[FechaRetiroUtc] IS NULL");b.HasIndex(x=>new{x.FechaRetiroUtc,x.UbicacionBateriaId});b.Property(x=>x.CodigoTubo).HasMaxLength(50);b.Property(x=>x.Gas).HasMaxLength(200);b.Property(x=>x.Unidad).HasMaxLength(20);b.Property(x=>x.CapacidadNominal).HasPrecision(18,3);b.Property(x=>x.ResultadoRetiro).HasMaxLength(20);b.Property(x=>x.UsuarioIngreso).HasMaxLength(256);b.Property(x=>x.UsuarioIngresoId).HasMaxLength(450);b.Property(x=>x.UsuarioRetiro).HasMaxLength(256);b.Property(x=>x.UsuarioRetiroId).HasMaxLength(450);b.Property(x=>x.MotivoRetiro).HasMaxLength(1000);b.HasOne(x=>x.Tubo).WithMany().HasForeignKey(x=>x.TuboId).OnDelete(DeleteBehavior.Restrict);b.HasOne(x=>x.UbicacionBateria).WithMany().HasForeignKey(x=>x.UbicacionBateriaId).OnDelete(DeleteBehavior.Restrict);}
}
