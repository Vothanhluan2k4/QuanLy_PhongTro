using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLy_PhongTro.Migrations
{
    /// <inheritdoc />
    public partial class addamount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Amount",
                table: "VnPayInfor",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "VnPayInfor");
        }
    }
}
