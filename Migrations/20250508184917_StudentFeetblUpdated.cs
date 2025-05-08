using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolBillingERP.Migrations
{
    /// <inheritdoc />
    public partial class StudentFeetblUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeeStatus",
                table: "StudentFees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeeStatus",
                table: "StudentFees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
