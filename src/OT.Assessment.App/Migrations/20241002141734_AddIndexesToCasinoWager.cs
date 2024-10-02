using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OT.Assessment.App.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesToCasinoWager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_CasinoWagers_AccountId",
                table: "CasinoWagers",
                newName: "IX_CasinoWager_AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Player_AccountId",
                table: "Players",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CasinoWager_WagerId",
                table: "CasinoWagers",
                column: "WagerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Player_AccountId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_CasinoWager_WagerId",
                table: "CasinoWagers");

            migrationBuilder.RenameIndex(
                name: "IX_CasinoWager_AccountId",
                table: "CasinoWagers",
                newName: "IX_CasinoWagers_AccountId");
        }
    }
}
