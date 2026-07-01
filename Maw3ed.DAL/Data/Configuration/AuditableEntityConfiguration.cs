using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public abstract class AuditableEntityConfiguration<T>
    : IEntityTypeConfiguration<T>
    where T : AuditableEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .HasMaxLength(450);

            builder.Property(x => x.UpdatedBy)
                .HasMaxLength(450);
        }
    }
}
