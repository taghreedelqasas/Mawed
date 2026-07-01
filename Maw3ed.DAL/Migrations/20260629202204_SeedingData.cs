using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Maw3ed.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SeedingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "BirthDate", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "Gender", "IsActive", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SSN", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "test-user-id-1", 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "patient@test.com", true, "يونس", null, true, "أحمد", false, null, "PATIENT@TEST.COM", "PATIENT@TEST.COM", null, null, false, "29801011234567", "11111111-1111-1111-1111-111111111111", false, "patient@test.com" },
                    { "test-user-id-2", 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "doctor@test.com", true, "أحمد", null, true, "السعيد", false, null, "DOCTOR@TEST.COM", "DOCTOR@TEST.COM", null, null, false, "29505051234567", "22222222-2222-2222-2222-222222222222", false, "doctor@test.com" }
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "طب عام" });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Address", "Certificate", "ConsultationFee", "CreatedAt", "CreatedBy", "DepartmentId", "GraduationDate", "IsVerified", "LicenseNumber", "UpdatedAt", "UpdatedBy", "UserId", "VerifiedAt", "VerifiedBy" },
                values: new object[] { 1, "القاهرة", "MD", 200m, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2010, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "LIC-001", null, null, "test-user-id-2", null, null });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "MedicalHistory", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[] { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "", null, null, "test-user-id-1" });

            migrationBuilder.InsertData(
                table: "Conversation",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DoctorId", "PatientId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, 1, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Conversation",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "test-user-id-1");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "test-user-id-2");

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
