using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductivitySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalUserMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GitHubLogin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrelloMemberId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalUserMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalUserMappings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUserMappings_UserId",
                table: "ExternalUserMappings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalUserMappings");
        }
    }
}
