using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public class PaymentConfiguration
    : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.SystemFee)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Status)
                .HasConversion<string>();

            builder.Property(x => x.Method)
                .HasConversion<string>();

            builder.HasOne(x => x.Appointment)
                .WithOne(x => x.Payment)
                .HasForeignKey<Payment>(x => x.AppointmentId);
        }
    }
}
