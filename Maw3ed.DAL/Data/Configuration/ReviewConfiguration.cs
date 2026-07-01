using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maw3ed.DAL
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Rating)
                .IsRequired();

            builder.Property(x => x.Comment)
                .HasMaxLength(1000);

            // ضمان: patient مينفعش يعمل أكتر من review لنفس الدكتور على مستوى الـ DB
            builder.HasIndex(x => new { x.PatientId, x.DoctorId })
                .IsUnique();

            // لو الـ Patient اتمسح → امسح reviews بتاعته
            builder.HasOne(x => x.Patient)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // لو الدكتور اتمسح → امسح reviews بتاعته
            // (SQL Server بيسمح بيها هنا لأن مفيش multiple cascade paths)
            builder.HasOne(x => x.Doctor)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
