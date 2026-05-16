using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductivitySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExternalTasksSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_ExternalSource_SourceId",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExternalSource",
                table: "ExternalSource");

            migrationBuilder.RenameTable(
                name: "ExternalSource",
                newName: "Sources");

            migrationBuilder.AddColumn<DateTime>(
                name: "SyncedAt",
                table: "Tasks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sources",
                table: "Sources",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Sources_SourceId",
                table: "Tasks",
                column: "SourceId",
                principalTable: "Sources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Sources_SourceId",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sources",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "SyncedAt",
                table: "Tasks");

            migrationBuilder.RenameTable(
                name: "Sources",
                newName: "ExternalSource");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExternalSource",
                table: "ExternalSource",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_ExternalSource_SourceId",
                table: "Tasks",
                column: "SourceId",
                principalTable: "ExternalSource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
