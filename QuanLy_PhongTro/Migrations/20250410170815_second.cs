using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLy_PhongTro.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HopDong_PhongTro_MaPhong",
                table: "HopDong");

            migrationBuilder.DropForeignKey(
                name: "FK_PhongTro_ThietBi_ThietBi_MaThietBi",
                table: "PhongTro_ThietBi");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "ThietBi",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "DonViTinh",
                table: "ThietBi",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "TinhTrang",
                table: "PhongTro_ThietBi",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "PhongTro_ThietBi",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayTao",
                table: "PhongTro",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "HopDong",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateTable(
                name: "PhanQuyens",
                columns: table => new
                {
                    MaQuyen = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenQuyen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanQuyens", x => x.MaQuyen);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungs",
                columns: table => new
                {
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MaQuyen = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungs", x => x.MaNguoiDung);
                    table.ForeignKey(
                        name: "FK_NguoiDungs_PhanQuyens_MaQuyen",
                        column: x => x.MaQuyen,
                        principalTable: "PhanQuyens",
                        principalColumn: "MaQuyen",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LuuPhongs",
                columns: table => new
                {
                    MaLuuPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhong = table.Column<int>(type: "int", nullable: false),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    NgayLuu = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LuuPhongs", x => x.MaLuuPhong);
                    table.ForeignKey(
                        name: "FK_LuuPhongs_NguoiDungs_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LuuPhongs_PhongTro_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "PhongTro",
                        principalColumn: "MaPhong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HopDong_MaNguoiDung",
                table: "HopDong",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_LuuPhongs_MaNguoiDung",
                table: "LuuPhongs",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_LuuPhongs_MaPhong",
                table: "LuuPhongs",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungs_Email",
                table: "NguoiDungs",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungs_MaQuyen",
                table: "NguoiDungs",
                column: "MaQuyen");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungs_SoDienThoai",
                table: "NguoiDungs",
                column: "SoDienThoai",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HopDong_NguoiDungs_MaNguoiDung",
                table: "HopDong",
                column: "MaNguoiDung",
                principalTable: "NguoiDungs",
                principalColumn: "MaNguoiDung",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HopDong_PhongTro_MaPhong",
                table: "HopDong",
                column: "MaPhong",
                principalTable: "PhongTro",
                principalColumn: "MaPhong",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PhongTro_ThietBi_ThietBi_MaThietBi",
                table: "PhongTro_ThietBi",
                column: "MaThietBi",
                principalTable: "ThietBi",
                principalColumn: "MaThietBi",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HopDong_NguoiDungs_MaNguoiDung",
                table: "HopDong");

            migrationBuilder.DropForeignKey(
                name: "FK_HopDong_PhongTro_MaPhong",
                table: "HopDong");

            migrationBuilder.DropForeignKey(
                name: "FK_PhongTro_ThietBi_ThietBi_MaThietBi",
                table: "PhongTro_ThietBi");

            migrationBuilder.DropTable(
                name: "LuuPhongs");

            migrationBuilder.DropTable(
                name: "NguoiDungs");

            migrationBuilder.DropTable(
                name: "PhanQuyens");

            migrationBuilder.DropIndex(
                name: "IX_HopDong_MaNguoiDung",
                table: "HopDong");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "ThietBi",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "DonViTinh",
                table: "ThietBi",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "TinhTrang",
                table: "PhongTro_ThietBi",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "PhongTro_ThietBi",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayTao",
                table: "PhongTro",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "HopDong",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddForeignKey(
                name: "FK_HopDong_PhongTro_MaPhong",
                table: "HopDong",
                column: "MaPhong",
                principalTable: "PhongTro",
                principalColumn: "MaPhong",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PhongTro_ThietBi_ThietBi_MaThietBi",
                table: "PhongTro_ThietBi",
                column: "MaThietBi",
                principalTable: "ThietBi",
                principalColumn: "MaThietBi",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
