using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSystem.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class fkMig3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "Notifications" (
                    "Id" uuid NOT NULL,
                    "UserId" text NOT NULL,
                    "Title" text NOT NULL,
                    "Body" text NOT NULL,
                    "IsRead" boolean NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id")
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS "Notifications";
                """);
        }
    }
}
