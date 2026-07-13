using Maw3ed.DAL.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Maw3ed.DAL
{
    public class AppDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<ChatSession> ChatSessions { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<DoctorWallet> DoctorWallets { get; set; }

        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        public DbSet<WithdrawRequest> WithdrawRequests { get; set; }
        public DbSet<MedicalReportAnalysis> MedicalReportAnalyses { get; set; }

        public DbSet<MedicalImageAnalysis> MedicalImageAnalyses { get; set; }

        public DbSet<Conversation> Conversations { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly);
        }


    }
}