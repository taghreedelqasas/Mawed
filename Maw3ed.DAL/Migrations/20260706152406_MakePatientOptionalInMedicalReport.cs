using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maw3ed.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MakePatientOptionalInMedicalReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalReportAnalyses_Patients_PatientId",
                table: "MedicalReportAnalyses");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "MedicalReportAnalyses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalReportAnalyses_Patients_PatientId",
                table: "MedicalReportAnalyses",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalReportAnalyses_Patients_PatientId",
                table: "MedicalReportAnalyses");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "MedicalReportAnalyses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalReportAnalyses_Patients_PatientId",
                table: "MedicalReportAnalyses",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
