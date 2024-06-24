using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAndMoidifiedDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Notes",
                newName: "LastModifiedDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Notes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Notes");

            migrationBuilder.RenameColumn(
                name: "LastModifiedDate",
                table: "Notes",
                newName: "Timestamp");
        }
    }
}
