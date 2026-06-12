using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEase.Migrations
{
    public partial class AddEventTypeAndImageUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ImageUrl (nullable)
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            // Add EventType (int) with default = 4 (Other)
            migrationBuilder.AddColumn<int>(
                name: "EventType",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 4);

            // Recreate consolidated view to include EventType
            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.vw_BookingConsolidated','V') IS NOT NULL
    DROP VIEW dbo.vw_BookingConsolidated;

CREATE VIEW dbo.vw_BookingConsolidated
AS
SELECT
    b.BookingId,
    e.EventId,
    e.EventName,
    v.VenueId,
    v.VenueName,
    b.BookingDate,
    e.EventDate,
    e.EventType
FROM dbo.Bookings AS b
INNER JOIN dbo.Events AS e ON b.EventId = e.EventId
INNER JOIN dbo.Venues AS v ON b.VenueId = v.VenueId;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop view if exists
            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.vw_BookingConsolidated','V') IS NOT NULL
    DROP VIEW dbo.vw_BookingConsolidated;
");

            // Remove EventType and ImageUrl columns
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Events");
        }
    }
}
