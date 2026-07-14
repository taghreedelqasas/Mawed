using Maw3ed.DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maw3ed.DAL.Data.Configuration
{
    public class PlatformSettingConfiguration : IEntityTypeConfiguration<PlatformSetting>
    {
        public void Configure(EntityTypeBuilder<PlatformSetting> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CommissionRate)
                .HasColumnType("decimal(5,2)");

            builder.Property(x => x.UpdatedBy)
                .HasMaxLength(450);

            // بذرة أولية: صف واحد بمعرف 1 ونسبة عمولة افتراضية 10%
            builder.HasData(new PlatformSetting
            {
                Id = 1,
                CommissionRate = 10m,
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedBy = null
            });
        }
    }
}