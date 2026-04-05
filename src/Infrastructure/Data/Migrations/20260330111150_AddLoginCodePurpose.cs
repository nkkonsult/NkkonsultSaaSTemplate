using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nkkonsult.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginCodePurpose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "purpose",
                table: "login_codes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "purpose",
                table: "login_codes");
        }
    }
}
