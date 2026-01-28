using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diiage.QuestService.Persistence.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InboxModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InboxStates",
                schema: "diiage-questservice-db",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsumerType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxStates", x => x.EventId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxStates_EventId_ConsumerType",
                schema: "diiage-questservice-db",
                table: "InboxStates",
                columns: new[] { "EventId", "ConsumerType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxStates",
                schema: "diiage-questservice-db");
        }
    }
}
