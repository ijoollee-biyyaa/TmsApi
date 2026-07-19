using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class AssessmentsConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
        .IsRequired()
        .HasMaxLength(200);

        builder.Property(a => a.MaxScore)
        .HasColumnType("decimal(6,2)");
        builder.Property(a => a.Weight)
        .HasColumnType("decimal(4,3)");

        builder.HasOne(a => a.Course)
        .WithMany(c=>c.Assessments)
        .HasForeignKey(a=>a.CourseId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.CourseId, a.Title })
        .IsUnique();
    }

}