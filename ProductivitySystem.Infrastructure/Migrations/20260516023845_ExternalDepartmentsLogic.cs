using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductivitySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExternalDepartmentsLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Departments");
        }
    }
}
