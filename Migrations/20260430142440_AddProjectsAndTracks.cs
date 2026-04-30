using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drum_Machine.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectsAndTracks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_Projects_ProjectEntityId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_ProjectEntityId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "ProjectEntityId",
                table: "Tracks");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_ProjectId",
                table: "Tracks",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_Projects_ProjectId",
                table: "Tracks",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_Projects_ProjectId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_ProjectId",
                table: "Tracks");

            migrationBuilder.AddColumn<int>(
                name: "ProjectEntityId",
                table: "Tracks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_ProjectEntityId",
                table: "Tracks",
                column: "ProjectEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_Projects_ProjectEntityId",
                table: "Tracks",
                column: "ProjectEntityId",
                principalTable: "Projects",
                principalColumn: "Id");
        }
    }
}
