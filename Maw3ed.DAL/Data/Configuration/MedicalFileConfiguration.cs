using Maw3ed.DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.Data.Configuration
{
    public class MedicalFileConfiguration : IEntityTypeConfiguration<MedicalFile>
    {
        public void Configure(EntityTypeBuilder<MedicalFile> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.FileUrl)
                .IsRequired();

            builder.Property(x => x.FileType)
                .IsRequired()
                .HasMaxLength(10);

            // الـ Category كـ string في الـ Database عشان يكون واضح
            builder.Property(x => x.Category)
                .HasConversion<string>();

            builder.Property(x => x.OcrStatus)
                .HasConversion<string>();

            // علاقة MedicalFile مع Patient
            builder.HasOne(x => x.Patient)
                .WithMany(x => x.MedicalFiles)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}