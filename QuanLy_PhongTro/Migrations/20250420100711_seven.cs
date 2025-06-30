using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLy_PhongTro.Migrations
{
    /// <inheritdoc />
    public partial class seven : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "PhongTro_ThietBi");

            migrationBuilder.DropColumn(
                name: "TinhTrang",
                table: "PhongTro_ThietBi");

            migrationBuilder.AddColumn<string>(
                name: "TinhTrang",
                table: "ThietBi",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TinhTrang",
                table: "ThietBi");

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "PhongTro_ThietBi",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TinhTrang",
                table: "PhongTro_ThietBi",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
