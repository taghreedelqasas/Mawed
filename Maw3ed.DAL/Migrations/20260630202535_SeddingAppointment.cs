using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maw3ed.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SeddingAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DoctorAvailabilities",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DoctorId", "EndTime", "IsBooked", "StartTime", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 7, 1, 11, 0, 0, 0, DateTimeKind.Unspecified), true, new DateTime(2026, 7, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), null, null });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DoctorAvailabilityId", "DoctorId", "Notes", "PatientId", "Status", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, 1, "موعد تيست", 1, "Confirmed", null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
