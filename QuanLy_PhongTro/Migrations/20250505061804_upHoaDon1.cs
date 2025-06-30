using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLy_PhongTro.Migrations
{
    /// <inheritdoc />
    public partial class upHoaDon1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_MomoInfor_IdMomo",
                table: "HoaDon");

            migrationBuilder.RenameColumn(
                name: "IdMomo",
                table: "MomoInfor",
                newName: "MomoId");

            migrationBuilder.RenameColumn(
                name: "IdMomo",
                table: "HoaDon",
                newName: "MomoId");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDon_IdMomo",
                table: "HoaDon",
                newName: "IX_HoaDon_MomoId");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_MomoInfor_MomoId",
                table: "HoaDon",
                column: "MomoId",
                principalTable: "MomoInfor",
                principalColumn: "MomoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_MomoInfor_MomoId",
                table: "HoaDon");

            migrationBuilder.RenameColumn(
                name: "MomoId",
                table: "MomoInfor",
                newName: "IdMomo");

            migrationBuilder.RenameColumn(
                name: "MomoId",
                table: "HoaDon",
                newName: "IdMomo");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDon_MomoId",
                table: "HoaDon",
                newName: "IX_HoaDon_IdMomo");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_MomoInfor_IdMomo",
                table: "HoaDon",
                column: "IdMomo",
                principalTable: "MomoInfor",
                principalColumn: "IdMomo");
        }
    }
}
