using Lantern.Domain.Regions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lantern.Infrastructure.Persistence.Configurations;

internal sealed class RegionConfiguration : IEntityTypeConfiguration<Region>
{
  public void Configure(EntityTypeBuilder<Region> builder)
  {
    builder.HasKey(r => r.Id);

    builder.Property(r => r.Id)
           .ValueGeneratedNever();

    builder.Property(r => r.Name)
           .HasMaxLength(200)
           .IsRequired();

    builder.Property(r => r.Type)
           .HasConversion<string>()
           .HasMaxLength(50)
           .IsRequired();

    builder.HasOne(r => r.ParentRegion)
           .WithMany(r => r.ChildRegions)
           .HasForeignKey(r => r.ParentRegionId)
           .OnDelete(DeleteBehavior.Restrict);
  }
}
