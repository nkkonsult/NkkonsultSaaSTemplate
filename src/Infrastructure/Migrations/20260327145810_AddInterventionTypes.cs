using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Nkkonsult.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInterventionTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "intervention_type_id",
                table: "interventions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "intervention_type_other",
                table: "interventions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "trades_ref",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    icon_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trades_ref", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "default_intervention_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    trade_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    icon_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_other = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_default_intervention_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_default_intervention_types_trades_trade_id",
                        column: x => x.trade_id,
                        principalTable: "trades_ref",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "intervention_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trade_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    icon_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_other = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_intervention_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_intervention_types_trades_trade_id",
                        column: x => x.trade_id,
                        principalTable: "trades_ref",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_interventions_intervention_type_id",
                table: "interventions",
                column: "intervention_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_default_intervention_types_trade_name",
                table: "default_intervention_types",
                columns: new[] { "trade_id", "name" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_intervention_types_tenant_name",
                table: "intervention_types",
                columns: new[] { "tenant_id", "name" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_intervention_types_tenant_trade",
                table: "intervention_types",
                columns: new[] { "tenant_id", "trade_id" });

            migrationBuilder.CreateIndex(
                name: "ix_intervention_types_trade_id",
                table: "intervention_types",
                column: "trade_id");

            migrationBuilder.CreateIndex(
                name: "ix_trades_ref_name",
                table: "trades_ref",
                column: "name",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.AddForeignKey(
                name: "fk_interventions_intervention_types_intervention_type_id",
                table: "interventions",
                column: "intervention_type_id",
                principalTable: "intervention_types",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_interventions_intervention_types_intervention_type_id",
                table: "interventions");

            migrationBuilder.DropTable(
                name: "default_intervention_types");

            migrationBuilder.DropTable(
                name: "intervention_types");

            migrationBuilder.DropTable(
                name: "trades_ref");

            migrationBuilder.DropIndex(
                name: "ix_interventions_intervention_type_id",
                table: "interventions");

            migrationBuilder.DropColumn(
                name: "intervention_type_id",
                table: "interventions");

            migrationBuilder.DropColumn(
                name: "intervention_type_other",
                table: "interventions");
        }
    }
}
