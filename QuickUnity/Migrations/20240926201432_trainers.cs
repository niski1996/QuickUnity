using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickUnity.Migrations
{
    /// <inheritdoc />
    public partial class trainers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrainerRowId",
                table: "ProfileRow",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrainerRow",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TrainerProfileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainerRow_ProfileRow_TrainerProfileId",
                        column: x => x.TrainerProfileId,
                        principalTable: "ProfileRow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileRow_TrainerRowId",
                table: "ProfileRow",
                column: "TrainerRowId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerRow_TrainerProfileId",
                table: "TrainerRow",
                column: "TrainerProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileRow_TrainerRow_TrainerRowId",
                table: "ProfileRow",
                column: "TrainerRowId",
                principalTable: "TrainerRow",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileRow_TrainerRow_TrainerRowId",
                table: "ProfileRow");

            migrationBuilder.DropTable(
                name: "TrainerRow");

            migrationBuilder.DropIndex(
                name: "IX_ProfileRow_TrainerRowId",
                table: "ProfileRow");

            migrationBuilder.DropColumn(
                name: "TrainerRowId",
                table: "ProfileRow");
        }
    }
}
