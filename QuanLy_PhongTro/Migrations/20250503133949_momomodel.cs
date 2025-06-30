using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLy_PhongTro.Migrations
{
    /// <inheritdoc />
    public partial class momomodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "MomoInfor",
                newName: "TransactionId");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "MomoInfor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "MomoInfor");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "MomoInfor",
                newName: "FullName");
        }
    }
}
