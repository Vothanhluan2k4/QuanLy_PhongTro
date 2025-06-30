using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLy_PhongTro.Migrations
{
    /// <inheritdoc />
    public partial class accountpaid1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountPaidInfor_HoaDon_MaHoaDon",
                table: "AccountPaidInfor");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountPaidInfor_NguoiDungs_MaNguoiDung",
                table: "AccountPaidInfor");

            migrationBuilder.AlterColumn<int>(
                name: "MaNguoiDung",
                table: "AccountPaidInfor",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaHoaDon",
                table: "AccountPaidInfor",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountPaidInfor_HoaDon_MaHoaDon",
                table: "AccountPaidInfor",
                column: "MaHoaDon",
                principalTable: "HoaDon",
                principalColumn: "MaHoaDon");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountPaidInfor_NguoiDungs_MaNguoiDung",
                table: "AccountPaidInfor",
                column: "MaNguoiDung",
                principalTable: "NguoiDungs",
                principalColumn: "MaNguoiDung",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountPaidInfor_HoaDon_MaHoaDon",
                table: "AccountPaidInfor");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountPaidInfor_NguoiDungs_MaNguoiDung",
                table: "AccountPaidInfor");

            migrationBuilder.AlterColumn<int>(
                name: "MaNguoiDung",
                table: "AccountPaidInfor",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MaHoaDon",
                table: "AccountPaidInfor",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountPaidInfor_HoaDon_MaHoaDon",
                table: "AccountPaidInfor",
                column: "MaHoaDon",
                principalTable: "HoaDon",
                principalColumn: "MaHoaDon",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountPaidInfor_NguoiDungs_MaNguoiDung",
                table: "AccountPaidInfor",
                column: "MaNguoiDung",
                principalTable: "NguoiDungs",
                principalColumn: "MaNguoiDung");
        }
    }
}
