using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookViews",
                columns: table => new
                {
                    BookId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastSeenEventId = table.Column<long>(type: "bigint", nullable: false),
                    LastViewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookViews", x => x.BookId);
                    table.ForeignKey(
                        name: "FK_BookViews_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BookViews");
        }
    }
}
