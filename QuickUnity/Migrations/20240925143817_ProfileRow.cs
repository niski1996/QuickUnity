using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickUnity.Migrations
{
    /// <inheritdoc />
    public partial class ProfileRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileRow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "NOW()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileRow", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileRow_UserId",
                table: "ProfileRow",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileRow");
        }
    }
}
