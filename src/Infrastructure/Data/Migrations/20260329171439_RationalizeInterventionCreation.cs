using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nkkonsult.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RationalizeInterventionCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Data fixup (phase 1): fix existing nullable columns before making them required
            migrationBuilder.Sql("UPDATE interventions SET scheduled_at = created_at WHERE scheduled_at IS NULL;");
            // Fix NULL intervention_type_id
            migrationBuilder.Sql(@"
                UPDATE interventions i
                SET intervention_type_id = (
                    SELECT it.id FROM intervention_types it
                    WHERE it.tenant_id = i.tenant_id AND it.is_deleted = false
                    ORDER BY it.priority ASC
                    LIMIT 1
                )
                WHERE i.intervention_type_id IS NULL;
            ");
            // Fix orphaned intervention_type_id (non-null but references non-existent type)
            migrationBuilder.Sql(@"
                UPDATE interventions i
                SET intervention_type_id = (
                    SELECT it.id FROM intervention_types it
                    WHERE it.tenant_id = i.tenant_id AND it.is_deleted = false
                    ORDER BY it.priority ASC
                    LIMIT 1
                )
                WHERE i.intervention_type_id IS NOT NULL
                  AND NOT EXISTS (
                    SELECT 1 FROM intervention_types it
                    WHERE it.id = i.intervention_type_id
                  );
            ");

            migrationBuilder.DropForeignKey(
                name: "fk_interventions_intervention_types_intervention_type_id",
                table: "interventions");

            migrationBuilder.DropColumn(
                name: "intervention_type_other",
                table: "interventions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "scheduled_at",
                table: "interventions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "intervention_type_id",
                table: "interventions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "duration_minutes",
                table: "interventions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "intervention_types",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            // Data fixup (phase 2): set defaults for newly added columns
            migrationBuilder.Sql("UPDATE interventions SET duration_minutes = 60 WHERE duration_minutes = 0;");
            migrationBuilder.Sql("UPDATE intervention_types SET is_active = true WHERE is_active IS NULL;");

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "clients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "clients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "zip_code",
                table: "clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_interventions_intervention_types_intervention_type_id",
                table: "interventions",
                column: "intervention_type_id",
                principalTable: "intervention_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_interventions_intervention_types_intervention_type_id",
                table: "interventions");

            migrationBuilder.DropColumn(
                name: "duration_minutes",
                table: "interventions");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "intervention_types");

            migrationBuilder.DropColumn(
                name: "city",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "country",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "zip_code",
                table: "clients");

            migrationBuilder.AlterColumn<DateTime>(
                name: "scheduled_at",
                table: "interventions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "intervention_type_id",
                table: "interventions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "intervention_type_other",
                table: "interventions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_interventions_intervention_types_intervention_type_id",
                table: "interventions",
                column: "intervention_type_id",
                principalTable: "intervention_types",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
