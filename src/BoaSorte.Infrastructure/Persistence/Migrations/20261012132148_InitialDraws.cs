using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoaSorte.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialDraws : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "draws",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    public_id = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    submitted_count = table.Column<long>(type: "bigint", nullable: false),
                    eligible_count = table.Column<long>(type: "bigint", nullable: false),
                    participants = table.Column<string[]>(type: "text[]", nullable: false),
                    winners = table.Column<string[]>(type: "text[]", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    range_end = table.Column<int>(type: "integer", nullable: true),
                    range_start = table.Column<int>(type: "integer", nullable: true),
                    remove_duplicates = table.Column<bool>(type: "boolean", nullable: false),
                    show_participants = table.Column<bool>(type: "boolean", nullable: false),
                    winners_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_draws", x => x.id);
                    table.CheckConstraint("ck_draws_kind", "kind IN ('list', 'range')");
                    table.CheckConstraint("ck_draws_range", "(kind = 'list' AND range_start IS NULL AND range_end IS NULL) OR (kind = 'range' AND range_start IS NOT NULL AND range_end >= range_start)");
                    table.CheckConstraint("ck_draws_winners_count", "winners_count >= 1");
                });

            migrationBuilder.CreateIndex(
                name: "ix_draws_expires_at",
                table: "draws",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_draws_public_id",
                table: "draws",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "draws");
        }
    }
}
