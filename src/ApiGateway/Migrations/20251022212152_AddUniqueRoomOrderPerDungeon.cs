using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueRoomOrderPerDungeon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_DungeonId",
                table: "Rooms");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_DungeonId_Order",
                table: "Rooms",
                columns: new[] { "DungeonId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_DungeonId_Order",
                table: "Rooms");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_DungeonId",
                table: "Rooms",
                column: "DungeonId");
        }
    }
}
