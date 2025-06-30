using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLy_PhongTro.Migrations
{
    public partial class hoaDonVnMomo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Xóa ràng buộc khóa ngoại từ bảng HoaDon
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_VnPayInfor_VnpayId",
                table: "HoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_MomoInfor_MomoId",
                table: "HoaDon");

            // Bước 2: Xóa ràng buộc khóa chính từ bảng VnPayInfor và MomoInfor
            migrationBuilder.DropPrimaryKey(
                name: "PK_VnPayInfor",
                table: "VnPayInfor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MomoInfor",
                table: "MomoInfor");

            // Bước 3: Thay đổi kiểu dữ liệu của cột VnpayId
            migrationBuilder.AlterColumn<long>(
                name: "VnpayId",
                table: "VnPayInfor",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "VnpayId",
                table: "HoaDon",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Bước 4: Thay đổi kiểu dữ liệu của cột MomoId
            migrationBuilder.AlterColumn<long>(
                name: "MomoId",
                table: "MomoInfor",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "MomoId",
                table: "HoaDon",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Bước 5: Tạo lại ràng buộc khóa chính
            migrationBuilder.AddPrimaryKey(
                name: "PK_VnPayInfor",
                table: "VnPayInfor",
                column: "VnpayId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MomoInfor",
                table: "MomoInfor",
                column: "MomoId");

            // Bước 6: Tạo lại ràng buộc khóa ngoại
            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_VnPayInfor_VnpayId",
                table: "HoaDon",
                column: "VnpayId",
                principalTable: "VnPayInfor",
                principalColumn: "VnpayId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_MomoInfor_MomoId",
                table: "HoaDon",
                column: "MomoId",
                principalTable: "MomoInfor",
                principalColumn: "MomoId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Xóa ràng buộc khóa ngoại từ bảng HoaDon
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_VnPayInfor_VnpayId",
                table: "HoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_MomoInfor_MomoId",
                table: "HoaDon");

            // Bước 2: Xóa ràng buộc khóa chính từ bảng VnPayInfor và MomoInfor
            migrationBuilder.DropPrimaryKey(
                name: "PK_VnPayInfor",
                table: "VnPayInfor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MomoInfor",
                table: "MomoInfor");

            // Bước 3: Đổi kiểu dữ liệu của cột VnpayId về int
            migrationBuilder.AlterColumn<int>(
                name: "VnpayId",
                table: "VnPayInfor",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "VnpayId",
                table: "HoaDon",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            // Bước 4: Đổi kiểu dữ liệu của cột MomoId về int
            migrationBuilder.AlterColumn<int>(
                name: "MomoId",
                table: "MomoInfor",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "MomoId",
                table: "HoaDon",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            // Bước 5: Tạo lại ràng buộc khóa chính
            migrationBuilder.AddPrimaryKey(
                name: "PK_VnPayInfor",
                table: "VnPayInfor",
                column: "VnpayId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MomoInfor",
                table: "MomoInfor",
                column: "MomoId");

            // Bước 6: Tạo lại ràng buộc khóa ngoại
            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_VnPayInfor_VnpayId",
                table: "HoaDon",
                column: "VnpayId",
                principalTable: "VnPayInfor",
                principalColumn: "VnpayId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_MomoInfor_MomoId",
                table: "HoaDon",
                column: "MomoId",
                principalTable: "MomoInfor",
                principalColumn: "MomoId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}