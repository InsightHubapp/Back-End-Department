using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightHub.Migrations
{
    public partial class FinalSync2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='InterviewQuestions' AND xtype='U')
                BEGIN
                    CREATE TABLE [InterviewQuestions] (
                        [Id] int NOT NULL IDENTITY,
                        [ExternalId] nvarchar(max) NOT NULL,
                        [Question] nvarchar(max) NOT NULL,
                        [Type] nvarchar(max) NOT NULL,
                        [Difficulty] nvarchar(max) NOT NULL,
                        [Explanation] nvarchar(max) NULL,
                        [Category] nvarchar(max) NOT NULL,
                        [Tags] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_InterviewQuestions] PRIMARY KEY ([Id])
                    )
                END

                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='InterviewQuestionOptions' AND xtype='U')
                BEGIN
                    CREATE TABLE [InterviewQuestionOptions] (
                        [Id] int NOT NULL IDENTITY,
                        [Option] nvarchar(max) NOT NULL,
                        [Number] int NOT NULL,
                        [IsCorrect] bit NOT NULL,
                        [InterviewQuestionId] int NOT NULL,
                        CONSTRAINT [PK_InterviewQuestionOptions] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_InterviewQuestionOptions_InterviewQuestions_InterviewQuestionId]
                            FOREIGN KEY ([InterviewQuestionId])
                            REFERENCES [InterviewQuestions] ([Id])
                            ON DELETE CASCADE
                    )
                END

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_InterviewQuestionOptions_InterviewQuestionId')
                BEGIN
                    CREATE INDEX [IX_InterviewQuestionOptions_InterviewQuestionId]
                    ON [InterviewQuestionOptions] ([InterviewQuestionId])
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "InterviewQuestionOptions");
            migrationBuilder.DropTable(name: "InterviewQuestions");
        }
    }
}