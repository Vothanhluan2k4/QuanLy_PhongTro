using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLy_PhongTro.Migrations
{
    /// <inheritdoc />
    public partial class adddata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_MomoInfor_MomoId",
                table: "HoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_VnPayInfor_VnpayId",
                table: "HoaDon");

            migrationBuilder.DropIndex(
                name: "IX_HoaDon_MomoId",
                table: "HoaDon");

            migrationBuilder.DropIndex(
                name: "IX_HoaDon_VnpayId",
                table: "HoaDon");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "VnPayInfor",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "VnPayInfor",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "VnPayInfor",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "VnPayInfor",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OrderDescription",
                table: "VnPayInfor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "VnPayInfor",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "MomoInfor",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "MomoInfor",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OrderInfo",
                table: "MomoInfor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "MomoInfor",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "MomoInfor",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "HopDong",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "HoaDon",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Chưa thanh toán",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MomoId",
                table: "HoaDon",
                column: "MomoId",
                unique: true,
                filter: "[MomoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_VnpayId",
                table: "HoaDon",
                column: "VnpayId",
                unique: true,
                filter: "[VnpayId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_MomoInfor_MomoId",
                table: "HoaDon",
                column: "MomoId",
                principalTable: "MomoInfor",
                principalColumn: "MomoId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_VnPayInfor_VnpayId",
                table: "HoaDon",
                column: "VnpayId",
                principalTable: "VnPayInfor",
                principalColumn: "VnpayId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_MomoInfor_MomoId",
                table: "HoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_VnPayInfor_VnpayId",
                table: "HoaDon");

            migrationBuilder.DropIndex(
                name: "IX_HoaDon_MomoId",
                table: "HoaDon");

            migrationBuilder.DropIndex(
                name: "IX_HoaDon_VnpayId",
                table: "HoaDon");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "VnPayInfor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "VnPayInfor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "VnPayInfor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "VnPayInfor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "OrderDescription",
                table: "VnPayInfor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "VnPayInfor",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "MomoInfor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "MomoInfor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "OrderInfo",
                table: "MomoInfor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "MomoInfor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "MomoInfor",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "HopDong",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "HoaDon",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Chưa thanh toán");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MomoId",
                table: "HoaDon",
                column: "MomoId");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_VnpayId",
                table: "HoaDon",
                column: "VnpayId");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_MomoInfor_MomoId",
                table: "HoaDon",
                column: "MomoId",
                principalTable: "MomoInfor",
                principalColumn: "MomoId");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_VnPayInfor_VnpayId",
                table: "HoaDon",
                column: "VnpayId",
                principalTable: "VnPayInfor",
                principalColumn: "VnpayId");
        }
    }
}
