using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLy_PhongTro.Migrations
{
    /// <inheritdoc />
    public partial class upHoaDon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "VnPayInfor",
                newName: "VnpayId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "MomoInfor",
                newName: "IdMomo");

            migrationBuilder.AddColumn<int>(
                name: "MaHoaDon",
                table: "VnPayInfor",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaHopDong",
                table: "VnPayInfor",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaHoaDon",
                table: "MomoInfor",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaHopDong",
                table: "MomoInfor",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChiSoDienNuoc",
                columns: table => new
                {
                    MaChiSo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHopDong = table.Column<int>(type: "int", nullable: false),
                    Thang = table.Column<int>(type: "int", nullable: false),
                    Nam = table.Column<int>(type: "int", nullable: false),
                    ChiSoDienCu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChiSoDienMoi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChiSoNuocCu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChiSoNuocMoi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayGhi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiSoDienNuoc", x => x.MaChiSo);
                    table.ForeignKey(
                        name: "FK_ChiSoDienNuoc_HopDong_MaHopDong",
                        column: x => x.MaHopDong,
                        principalTable: "HopDong",
                        principalColumn: "MaHopDong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    MaHoaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHopDong = table.Column<int>(type: "int", nullable: false),
                    Thang = table.Column<int>(type: "int", nullable: false),
                    Nam = table.Column<int>(type: "int", nullable: false),
                    TienDien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TienNuoc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TienRac = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayPhatHanh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IdMomo = table.Column<int>(type: "int", nullable: true),
                    VnpayId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDon", x => x.MaHoaDon);
                    table.ForeignKey(
                        name: "FK_HoaDon_HopDong_MaHopDong",
                        column: x => x.MaHopDong,
                        principalTable: "HopDong",
                        principalColumn: "MaHopDong",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoaDon_MomoInfor_IdMomo",
                        column: x => x.IdMomo,
                        principalTable: "MomoInfor",
                        principalColumn: "IdMomo");
                    table.ForeignKey(
                        name: "FK_HoaDon_VnPayInfor_VnpayId",
                        column: x => x.VnpayId,
                        principalTable: "VnPayInfor",
                        principalColumn: "VnpayId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_VnPayInfor_MaHoaDon",
                table: "VnPayInfor",
                column: "MaHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_VnPayInfor_MaHopDong",
                table: "VnPayInfor",
                column: "MaHopDong");

            migrationBuilder.CreateIndex(
                name: "IX_MomoInfor_MaHoaDon",
                table: "MomoInfor",
                column: "MaHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_MomoInfor_MaHopDong",
                table: "MomoInfor",
                column: "MaHopDong");

            migrationBuilder.CreateIndex(
                name: "IX_ChiSoDienNuoc_MaHopDong",
                table: "ChiSoDienNuoc",
                column: "MaHopDong");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_IdMomo",
                table: "HoaDon",
                column: "IdMomo");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaHopDong",
                table: "HoaDon",
                column: "MaHopDong");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_VnpayId",
                table: "HoaDon",
                column: "VnpayId");

            migrationBuilder.AddForeignKey(
                name: "FK_MomoInfor_HoaDon_MaHoaDon",
                table: "MomoInfor",
                column: "MaHoaDon",
                principalTable: "HoaDon",
                principalColumn: "MaHoaDon");

            migrationBuilder.AddForeignKey(
                name: "FK_MomoInfor_HopDong_MaHopDong",
                table: "MomoInfor",
                column: "MaHopDong",
                principalTable: "HopDong",
                principalColumn: "MaHopDong");

            migrationBuilder.AddForeignKey(
                name: "FK_VnPayInfor_HoaDon_MaHoaDon",
                table: "VnPayInfor",
                column: "MaHoaDon",
                principalTable: "HoaDon",
                principalColumn: "MaHoaDon");

            migrationBuilder.AddForeignKey(
                name: "FK_VnPayInfor_HopDong_MaHopDong",
                table: "VnPayInfor",
                column: "MaHopDong",
                principalTable: "HopDong",
                principalColumn: "MaHopDong");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MomoInfor_HoaDon_MaHoaDon",
                table: "MomoInfor");

            migrationBuilder.DropForeignKey(
                name: "FK_MomoInfor_HopDong_MaHopDong",
                table: "MomoInfor");

            migrationBuilder.DropForeignKey(
                name: "FK_VnPayInfor_HoaDon_MaHoaDon",
                table: "VnPayInfor");

            migrationBuilder.DropForeignKey(
                name: "FK_VnPayInfor_HopDong_MaHopDong",
                table: "VnPayInfor");

            migrationBuilder.DropTable(
                name: "ChiSoDienNuoc");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropIndex(
                name: "IX_VnPayInfor_MaHoaDon",
                table: "VnPayInfor");

            migrationBuilder.DropIndex(
                name: "IX_VnPayInfor_MaHopDong",
                table: "VnPayInfor");

            migrationBuilder.DropIndex(
                name: "IX_MomoInfor_MaHoaDon",
                table: "MomoInfor");

            migrationBuilder.DropIndex(
                name: "IX_MomoInfor_MaHopDong",
                table: "MomoInfor");

            migrationBuilder.DropColumn(
                name: "MaHoaDon",
                table: "VnPayInfor");

            migrationBuilder.DropColumn(
                name: "MaHopDong",
                table: "VnPayInfor");

            migrationBuilder.DropColumn(
                name: "MaHoaDon",
                table: "MomoInfor");

            migrationBuilder.DropColumn(
                name: "MaHopDong",
                table: "MomoInfor");

            migrationBuilder.RenameColumn(
                name: "VnpayId",
                table: "VnPayInfor",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdMomo",
                table: "MomoInfor",
                newName: "Id");
        }
    }
}
