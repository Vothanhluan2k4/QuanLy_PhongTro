using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLy_PhongTro.Migrations
{
    /// <inheritdoc />
    public partial class upHoaDon2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MomoInfor_HoaDon_MaHoaDon",
                table: "MomoInfor");

            migrationBuilder.DropForeignKey(
                name: "FK_VnPayInfor_HoaDon_MaHoaDon",
                table: "VnPayInfor");

            migrationBuilder.DropIndex(
                name: "IX_VnPayInfor_MaHoaDon",
                table: "VnPayInfor");

            migrationBuilder.DropIndex(
                name: "IX_MomoInfor_MaHoaDon",
                table: "MomoInfor");

            migrationBuilder.DropColumn(
                name: "MaHoaDon",
                table: "VnPayInfor");

            migrationBuilder.DropColumn(
                name: "MaHoaDon",
                table: "MomoInfor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaHoaDon",
                table: "VnPayInfor",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaHoaDon",
                table: "MomoInfor",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VnPayInfor_MaHoaDon",
                table: "VnPayInfor",
                column: "MaHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_MomoInfor_MaHoaDon",
                table: "MomoInfor",
                column: "MaHoaDon");

            migrationBuilder.AddForeignKey(
                name: "FK_MomoInfor_HoaDon_MaHoaDon",
                table: "MomoInfor",
                column: "MaHoaDon",
                principalTable: "HoaDon",
                principalColumn: "MaHoaDon");

            migrationBuilder.AddForeignKey(
                name: "FK_VnPayInfor_HoaDon_MaHoaDon",
                table: "VnPayInfor",
                column: "MaHoaDon",
                principalTable: "HoaDon",
                principalColumn: "MaHoaDon");
        }
    }
}
