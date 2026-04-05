using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hoplo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArticlesPerTrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "unit_price",
                table: "articles",
                type: "decimal",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "articles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "articles",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<int>(
                name: "trade_id",
                table: "articles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Data migration: set existing articles to Serrurier trade (id=1 from TradeSeeder)
            migrationBuilder.Sql("""
                UPDATE articles SET trade_id = (SELECT id FROM trades_ref WHERE name = 'Serrurier' LIMIT 1)
                WHERE trade_id = 0;
                """);

            migrationBuilder.CreateTable(
                name: "default_articles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    trade_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_default_articles", x => x.id);
                    table.ForeignKey(
                        name: "fk_default_articles_trades_trade_id",
                        column: x => x.trade_id,
                        principalTable: "trades_ref",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_articles_tenant_name",
                table: "articles",
                columns: new[] { "tenant_id", "name" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_articles_tenant_trade",
                table: "articles",
                columns: new[] { "tenant_id", "trade_id" });

            migrationBuilder.CreateIndex(
                name: "ix_articles_trade_id",
                table: "articles",
                column: "trade_id");

            migrationBuilder.CreateIndex(
                name: "ix_default_articles_trade_name",
                table: "default_articles",
                columns: new[] { "trade_id", "name" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.AddForeignKey(
                name: "fk_articles_trades_trade_id",
                table: "articles",
                column: "trade_id",
                principalTable: "trades_ref",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_articles_trades_trade_id",
                table: "articles");

            migrationBuilder.DropTable(
                name: "default_articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_tenant_name",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_tenant_trade",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_articles_trade_id",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "trade_id",
                table: "articles");

            migrationBuilder.AlterColumn<decimal>(
                name: "unit_price",
                table: "articles",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "articles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "articles",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);
        }
    }
}
