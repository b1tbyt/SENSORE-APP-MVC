using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensoreApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PatientName",
                table: "PatientRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "PatientID",
                table: "PatientRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PatientID",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientID",
                table: "PatientRecords");

            migrationBuilder.DropColumn(
                name: "PatientID",
                table: "Appointments");

            migrationBuilder.AlterColumn<string>(
                name: "PatientName",
                table: "PatientRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
