using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OAIM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class transfaeCustomerTypeFromCustomerToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerType",
                table: "Customers");

            migrationBuilder.AddColumn<int>(
                name: "CustomerType",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerType",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "CustomerType",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
