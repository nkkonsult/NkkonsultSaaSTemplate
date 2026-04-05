using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nkkonsult.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantCompanyFieldsAndProfilePhotoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "postal_code",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "siren",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vat_number",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profile_photo_url",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "city",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "postal_code",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "siren",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "vat_number",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "profile_photo_url",
                table: "AspNetUsers");
        }
    }
}
