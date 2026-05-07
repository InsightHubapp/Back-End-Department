using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightHub.Migrations
{
    /// <inheritdoc />
    public partial class AddHasCompletedAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCompletedAssessment",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCompletedAssessment",
                table: "AspNetUsers");
        }
    }
}

