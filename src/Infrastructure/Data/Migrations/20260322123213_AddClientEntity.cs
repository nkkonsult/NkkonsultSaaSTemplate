using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nkkonsult.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Purge existing test data (interventions reference old client columns)
            migrationBuilder.Sql("DELETE FROM photos;");
            migrationBuilder.Sql("DELETE FROM lignes_devis;");
            migrationBuilder.Sql("DELETE FROM devis;");
            migrationBuilder.Sql("DELETE FROM interventions;");

            migrationBuilder.DropColumn(
                name: "client_address",
                table: "interventions");

            migrationBuilder.DropColumn(
                name: "client_email",
                table: "interventions");

            migrationBuilder.DropColumn(
                name: "client_name",
                table: "interventions");

            migrationBuilder.DropColumn(
                name: "client_phone",
                table: "interventions");

            migrationBuilder.AddColumn<Guid>(
                name: "client_id",
                table: "interventions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    internal_number = table.Column<int>(type: "integer", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clients", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_interventions_client_id",
                table: "interventions",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_created",
                table: "clients",
                columns: new[] { "tenant_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_internal_number",
                table: "clients",
                columns: new[] { "tenant_id", "internal_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_phone",
                table: "clients",
                columns: new[] { "tenant_id", "phone" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.AddForeignKey(
                name: "fk_interventions_clients_client_id",
                table: "interventions",
                column: "client_id",
                principalTable: "clients",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_interventions_clients_client_id",
                table: "interventions");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropIndex(
                name: "ix_interventions_client_id",
                table: "interventions");

            migrationBuilder.DropColumn(
                name: "client_id",
                table: "interventions");

            migrationBuilder.AddColumn<string>(
                name: "client_address",
                table: "interventions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "client_email",
                table: "interventions",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "client_name",
                table: "interventions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "client_phone",
                table: "interventions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
