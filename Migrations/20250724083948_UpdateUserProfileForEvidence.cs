using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssessmentPlatform.Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProfileForEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLinkedInVerified",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "WorkExperience",
                table: "UserProfiles",
                newName: "WorkExperienceText");

            migrationBuilder.RenameColumn(
                name: "Skills",
                table: "UserProfiles",
                newName: "WorkExperienceStatus");

            migrationBuilder.RenameColumn(
                name: "Education",
                table: "UserProfiles",
                newName: "SkillsText");

            migrationBuilder.AddColumn<string[]>(
                name: "EducationEvidence",
                table: "UserProfiles",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "EducationStatus",
                table: "UserProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EducationText",
                table: "UserProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string[]>(
                name: "SkillsEvidence",
                table: "UserProfiles",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "SkillsStatus",
                table: "UserProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string[]>(
                name: "WorkExperienceEvidence",
                table: "UserProfiles",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EducationEvidence",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "EducationStatus",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "EducationText",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SkillsEvidence",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SkillsStatus",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "WorkExperienceEvidence",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "WorkExperienceText",
                table: "UserProfiles",
                newName: "WorkExperience");

            migrationBuilder.RenameColumn(
                name: "WorkExperienceStatus",
                table: "UserProfiles",
                newName: "Skills");

            migrationBuilder.RenameColumn(
                name: "SkillsText",
                table: "UserProfiles",
                newName: "Education");

            migrationBuilder.AddColumn<bool>(
                name: "IsLinkedInVerified",
                table: "UserProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
