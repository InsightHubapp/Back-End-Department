using System;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable

namespace InsightHub.Migrations
{
    public partial class F : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ? JobOffers - only create if not exists
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='JobOffers' AND xtype='U')
                BEGIN
                    CREATE TABLE [JobOffers] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [ExternalId] nvarchar(450) NOT NULL,
                        [Title] nvarchar(max) NOT NULL,
                        [CompanyName] nvarchar(max) NOT NULL,
                        [Location] nvarchar(max) NOT NULL,
                        [Description] nvarchar(max) NOT NULL,
                        [Category] nvarchar(max) NULL,
                        [RedirectUrl] nvarchar(max) NOT NULL,
                        [CreatedDate] datetime2 NULL,
                        [FetchedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_JobOffers] PRIMARY KEY ([Id])
                    );
                END
            ");

            // ? Index on JobOffers
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_JobOffers_ExternalId' AND object_id = OBJECT_ID('JobOffers'))
                BEGIN
                    CREATE UNIQUE INDEX [IX_JobOffers_ExternalId] ON [JobOffers] ([ExternalId]);
                END
            ");

            // ? NewsArticles - only create if not exists
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsArticles' AND xtype='U')
                BEGIN
                    CREATE TABLE [NewsArticles] (
                        [Id] nvarchar(450) NOT NULL,
                        [Title] nvarchar(max) NOT NULL,
                        [Url] nvarchar(max) NOT NULL,
                        [ImageUrl] nvarchar(max) NOT NULL,
                        [SourceName] nvarchar(max) NOT NULL,
                        [PublishedAt] datetime2 NOT NULL,
                        [Track] nvarchar(max) NOT NULL,
                        [Tags] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_NewsArticles] PRIMARY KEY ([Id])
                    );
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "JobOffers");
            migrationBuilder.DropTable(name: "NewsArticles");
        }
    }
}
