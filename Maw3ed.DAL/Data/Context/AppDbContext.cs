using Maw3ed.DAL.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
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
        public DbSet<MedicalFile> MedicalFiles { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<ChatSession> ChatSessions { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // ============ SEED DATA ============

            modelBuilder.Entity<ApplicationUser>().HasData(

                new ApplicationUser
                {
                    Id = "test-user-id-1",
                    FirstName = "يونس",
                    LastName = "أحمد",
                    SSN = "29801011234567",

                    Email = "patient@test.com",
                    NormalizedEmail = "PATIENT@TEST.COM",

                    UserName = "patient@test.com",
                    NormalizedUserName = "PATIENT@TEST.COM",

                    IsActive = true,
                    EmailConfirmed = true,

                    SecurityStamp = "11111111-1111-1111-1111-111111111111",
                    ConcurrencyStamp = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",

                    //PasswordHash = "PUT_HASH_HERE"
                },

                new ApplicationUser
                {
                    Id = "test-user-id-2",
                    FirstName = "أحمد",
                    LastName = "السعيد",
                    SSN = "29505051234567",

                    Email = "doctor@test.com",
                    NormalizedEmail = "DOCTOR@TEST.COM",

                    UserName = "doctor@test.com",
                    NormalizedUserName = "DOCTOR@TEST.COM",

                    IsActive = true,
                    EmailConfirmed = true,

                    SecurityStamp = "22222222-2222-2222-2222-222222222222",
                    ConcurrencyStamp = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",

                    //PasswordHash = "PUT_HASH_HERE"
                }
            );

            modelBuilder.Entity<Department>().HasData(
                new Department
                {
                    Id = 1,
                    Name = "طب عام"
                });

            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    Id = 1,
                    UserId = "test-user-id-1",
                    MedicalHistory = ""
                });

            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    Id = 1,
                    UserId = "test-user-id-2",
                    LicenseNumber = "LIC-001",
                    Certificate = "MD",
                    ConsultationFee = 200,
                    Address = "القاهرة",
                    IsVerified = true,
                    GraduationDate = new DateTime(2010, 1, 1),
                    DepartmentId = 1
                });

            modelBuilder.Entity<Conversation>().HasData(
                new Conversation
                {
                    Id = 1,
                    PatientId = 1,
                    DoctorId = 1
            
                
                });

            // DoctorAvailability وهمية
            modelBuilder.Entity<DoctorAvailability>().HasData(
                new DoctorAvailability
                {
                    Id = 1,
                    DoctorId = 1,
                    StartTime = new DateTime(2026, 7, 1, 10, 0, 0),
                    EndTime = new DateTime(2026, 7, 1, 11, 0, 0),
                    IsBooked = true
                }
            );

            // Appointment وهمية بين Patient Id=1 و Doctor Id=1
            modelBuilder.Entity<Appointment>().HasData(
                new Appointment
                {
                    Id = 1,
                    PatientId = 1,
                    DoctorId = 1,
                    DoctorAvailabilityId = 1,
                    Status = AppointmentStatus.Confirmed,
                    Notes = "موعد تيست"
                }
            );
        }

    }
}
