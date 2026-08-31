using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        /// <inheritdoc />
        ///
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "TEXT",
                        nullable: false),

                    UserId = table.Column<Guid>(
                        type: "TEXT",
                        nullable: false),

                    TokenHash = table.Column<string>(
                        type: "TEXT",
                        nullable: false),

                    ExpiresAtUtc = table.Column<DateTime>(
                        type: "TEXT",
                        nullable: false),

                    CreatedAtUtc = table.Column<DateTime>(
                        type: "TEXT",
                        nullable: false),

                    IsActive = table.Column<bool>(
                        type: "INTEGER",
                        nullable: false),

                    LastUsedAtUtc = table.Column<DateTime>(
                        type: "TEXT",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_RefreshTokens",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");
        }
    }
}
