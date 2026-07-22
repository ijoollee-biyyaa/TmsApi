using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
      builder.HasKey(s=>s.Id);
      builder.Property(s=>s.RegistrationNumber)
      .HasMaxLength(50).IsRequired();
      builder.HasIndex(s=>s.RegistrationNumber)
      .IsUnique();
      builder.Property(s=>s.Name)
      .IsRequired()
      .HasMaxLength(200);
      builder.Property(s=>s.GPA)
      .HasColumnType("decimal(3,2)");
      builder.HasQueryFilter(s=>!s.IsDeleted);
      builder.Property<DateTime>("LastUpdated");
      builder.Property(s=>s.Version).IsRowVersion();
     
    }
}