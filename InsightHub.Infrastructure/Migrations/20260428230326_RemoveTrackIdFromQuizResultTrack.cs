using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightHub.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTrackIdFromQuizResultTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizResults_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizResultTracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizResultId = table.Column<int>(type: "int", nullable: false),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    TrackName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredSkills = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<double>(type: "float", nullable: false),
                    TrackSimilarityScore = table.Column<double>(type: "float", nullable: false),
                    SimilarityMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalEmployeesInTrack = table.Column<int>(type: "int", nullable: false),
                    AvgTechnicalLevel = table.Column<double>(type: "float", nullable: false),
                    AvgSoftSkills = table.Column<double>(type: "float", nullable: false),
                    AvgSalarySatisfaction = table.Column<double>(type: "float", nullable: false),
                    AvgWorkLifeBalance = table.Column<double>(type: "float", nullable: false),
                    MostCommonEnvironment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MostCommonCompanySize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvgYearsExperience = table.Column<double>(type: "float", nullable: false),
                    AvgConsistency = table.Column<double>(type: "float", nullable: false),
                    AvgAdaptability = table.Column<double>(type: "float", nullable: false),
                    AvgTeamwork = table.Column<double>(type: "float", nullable: false),
                    AvgProblemSolving = table.Column<double>(type: "float", nullable: false),
                    AvgLearningProactivity = table.Column<double>(type: "float", nullable: false),
                    AvgCommunication = table.Column<double>(type: "float", nullable: false),
                    AvgPrioritization = table.Column<double>(type: "float", nullable: false),
                    AvgOwnership = table.Column<double>(type: "float", nullable: false),
                    AvgCollaboration = table.Column<double>(type: "float", nullable: false),
                    AvgResilience = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizResultTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizResultTracks_QuizResults_QuizResultId",
                        column: x => x.QuizResultId,
                        principalTable: "QuizResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizResults_UserId",
                table: "QuizResults",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizResultTracks_QuizResultId",
                table: "QuizResultTracks",
                column: "QuizResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizResultTracks");

            migrationBuilder.DropTable(
                name: "QuizResults");
        }
    }
}

