using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLy_PhongTro.Migrations
{
    /// <inheritdoc />
    public partial class relationshipDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SuCos_NguoiDungs_MaNguoiDung",
                table: "SuCos");

            migrationBuilder.DropForeignKey(
                name: "FK_SuCos_PhongTro_MaPhong",
                table: "SuCos");

            migrationBuilder.AddForeignKey(
                name: "FK_SuCos_NguoiDungs_MaNguoiDung",
                table: "SuCos",
                column: "MaNguoiDung",
                principalTable: "NguoiDungs",
                principalColumn: "MaNguoiDung",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SuCos_PhongTro_MaPhong",
                table: "SuCos",
                column: "MaPhong",
                principalTable: "PhongTro",
                principalColumn: "MaPhong",
                onDelete: ReferentialAction.Restrict);
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
    }
}
