using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLy_PhongTro.Migrations
{
    /// <inheritdoc />
    public partial class updateSuco1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SuCos_NguoiDungs_ReporterId",
                table: "SuCos");

            migrationBuilder.DropForeignKey(
                name: "FK_SuCos_PhongTro_RoomId",
                table: "SuCos");

            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "SuCos",
                newName: "MaPhong");

            migrationBuilder.RenameColumn(
                name: "ReporterId",
                table: "SuCos",
                newName: "MaNguoiDung");

            migrationBuilder.RenameIndex(
                name: "IX_SuCos_RoomId",
                table: "SuCos",
                newName: "IX_SuCos_MaPhong");

            migrationBuilder.RenameIndex(
                name: "IX_SuCos_ReporterId",
                table: "SuCos",
                newName: "IX_SuCos_MaNguoiDung");

            migrationBuilder.AddForeignKey(
                name: "FK_SuCos_NguoiDungs_MaNguoiDung",
                table: "SuCos",
                column: "MaNguoiDung",
                principalTable: "NguoiDungs",
                principalColumn: "MaNguoiDung",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SuCos_PhongTro_MaPhong",
                table: "SuCos",
                column: "MaPhong",
                principalTable: "PhongTro",
                principalColumn: "MaPhong",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SuCos_NguoiDungs_MaNguoiDung",
                table: "SuCos");

            migrationBuilder.DropForeignKey(
                name: "FK_SuCos_PhongTro_MaPhong",
                table: "SuCos");

            migrationBuilder.RenameColumn(
                name: "MaPhong",
                table: "SuCos",
                newName: "RoomId");

            migrationBuilder.RenameColumn(
                name: "MaNguoiDung",
                table: "SuCos",
                newName: "ReporterId");

            migrationBuilder.RenameIndex(
                name: "IX_SuCos_MaPhong",
                table: "SuCos",
                newName: "IX_SuCos_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_SuCos_MaNguoiDung",
                table: "SuCos",
                newName: "IX_SuCos_ReporterId");

            migrationBuilder.AddForeignKey(
                name: "FK_SuCos_NguoiDungs_ReporterId",
                table: "SuCos",
                column: "ReporterId",
                principalTable: "NguoiDungs",
                principalColumn: "MaNguoiDung",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SuCos_PhongTro_RoomId",
                table: "SuCos",
                column: "RoomId",
                principalTable: "PhongTro",
                principalColumn: "MaPhong",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
