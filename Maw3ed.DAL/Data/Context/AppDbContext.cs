using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Maw3ed.DAL
{
    public class AppDbContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Doctor>             Doctors              { get; set; } = null!;
        public DbSet<Patient>            Patients             { get; set; } = null!;
        public DbSet<Department>         Departments          { get; set; } = null!;
        public DbSet<Appointment>        Appointments         { get; set; } = null!;
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; } = null!;
        public DbSet<Payment>            Payments             { get; set; } = null!;
        public DbSet<Review>             Reviews              { get; set; } = null!;
        public DbSet<ChatSession>        ChatSessions         { get; set; } = null!;
        public DbSet<ChatMessage>        ChatMessages         { get; set; } = null!;
        public DbSet<Conversation>       Conversations        { get; set; } = null!;
        public DbSet<Message>            Messages             { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        // ── Audit fields: يُملأ تلقائياً عند كل Save ─────────────────────
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            StampAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            StampAuditFields();
            return base.SaveChanges();
        }

        private void StampAuditFields()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                        break;
                }
            }
        }
    }
}
