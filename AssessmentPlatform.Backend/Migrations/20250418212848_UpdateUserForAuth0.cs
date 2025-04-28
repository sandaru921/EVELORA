using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssessmentPlatform.Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserForAuth0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Users",
                newName: "Email");

            migrationBuilder.AddColumn<string>(
                name: "Auth0Id",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Auth0Id",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "Password");
        }
    }
}
