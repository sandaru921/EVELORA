using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssessmentPlatform.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkModeToJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkMode",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkMode",
                table: "Jobs");
        }
    }
}
