using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nkkonsult.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientEmailToIntervention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "client_email",
                table: "interventions",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "client_email",
                table: "interventions");
        }
    }
}
