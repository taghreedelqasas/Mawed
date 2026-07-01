using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public class DoctorConfiguration
    : AuditableEntityConfiguration<Doctor>, IEntityTypeConfiguration<Doctor> 
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LicenseNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Certificate)
                .HasMaxLength(500);

            builder.Property(x => x.Address)
                .HasMaxLength(250);

            builder.Property(x => x.ConsultationFee)
                .HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.User)
                .WithOne(x => x.Doctor)
                .HasForeignKey<Doctor>(x => x.UserId);

            builder.HasOne(x => x.Department)
                .WithMany(x => x.Doctors)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
