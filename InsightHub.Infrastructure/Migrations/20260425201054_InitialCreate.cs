using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightHub.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoles' AND xtype='U')
                BEGIN
                    CREATE TABLE [AspNetRoles] (
                        [Id] nvarchar(450) NOT NULL,
                        [Name] nvarchar(256) NULL,
                        [NormalizedName] nvarchar(256) NULL,
                        [ConcurrencyStamp] nvarchar(max) NULL,
                        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Tracks' AND xtype='U')
                BEGIN
                    CREATE TABLE [Tracks] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [Name] nvarchar(100) NOT NULL,
                        [Description] nvarchar(500) NOT NULL,
                        [RequiredSkills] nvarchar(500) NOT NULL,
                        CONSTRAINT [PK_Tracks] PRIMARY KEY ([Id])
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoleClaims' AND xtype='U')
                BEGIN
                    CREATE TABLE [AspNetRoleClaims] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [RoleId] nvarchar(450) NOT NULL,
                        [ClaimType] nvarchar(max) NULL,
                        [ClaimValue] nvarchar(max) NULL,
                        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId])
                            REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUsers' AND xtype='U')
                BEGIN
                    CREATE TABLE [AspNetUsers] (
                        [Id] nvarchar(450) NOT NULL,
                        [FirstName] nvarchar(50) NOT NULL,
                        [LastName] nvarchar(50) NOT NULL,
                        [Gender] int NOT NULL,
                        [BirthDate] datetime2 NULL,
                        [Collage] nvarchar(50) NULL,
                        [IsEmployed] bit NOT NULL,
                        [TrackId] int NULL,
                        [YearsExperience] int NULL,
                        [UserName] nvarchar(256) NULL,
                        [NormalizedUserName] nvarchar(256) NULL,
                        [Email] nvarchar(256) NULL,
                        [NormalizedEmail] nvarchar(256) NULL,
                        [EmailConfirmed] bit NOT NULL,
                        [PasswordHash] nvarchar(max) NULL,
                        [SecurityStamp] nvarchar(max) NULL,
                        [ConcurrencyStamp] nvarchar(max) NULL,
                        [PhoneNumber] nvarchar(max) NULL,
                        [PhoneNumberConfirmed] bit NOT NULL,
                        [TwoFactorEnabled] bit NOT NULL,
                        [LockoutEnd] datetimeoffset NULL,
                        [LockoutEnabled] bit NOT NULL,
                        [AccessFailedCount] int NOT NULL,
                        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_AspNetUsers_Tracks_TrackId] FOREIGN KEY ([TrackId])
                            REFERENCES [Tracks] ([Id])
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CategoryLabels' AND xtype='U')
                BEGIN
                    CREATE TABLE [CategoryLabels] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [Name] nvarchar(max) NOT NULL,
                        [TrackId] int NOT NULL,
                        CONSTRAINT [PK_CategoryLabels] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_CategoryLabels_Tracks_TrackId] FOREIGN KEY ([TrackId])
                            REFERENCES [Tracks] ([Id]) ON DELETE CASCADE
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Questions' AND xtype='U')
                BEGIN
                    CREATE TABLE [Questions] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [Text] nvarchar(300) NOT NULL,
                        [Type] int NOT NULL,
                        [AppliesTo] int NOT NULL,
                        [Order] int NOT NULL,
                        [MaxValue] int NULL,
                        [IsCareerQuiz] bit NOT NULL,
                        [TrackId] int NULL,
                        CONSTRAINT [PK_Questions] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Questions_Tracks_TrackId] FOREIGN KEY ([TrackId])
                            REFERENCES [Tracks] ([Id])
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserClaims' AND xtype='U')
                BEGIN
                    CREATE TABLE [AspNetUserClaims] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [UserId] nvarchar(450) NOT NULL,
                        [ClaimType] nvarchar(max) NULL,
                        [ClaimValue] nvarchar(max) NULL,
                        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId])
                            REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserLogins' AND xtype='U')
                BEGIN
                    CREATE TABLE [AspNetUserLogins] (
                        [LoginProvider] nvarchar(450) NOT NULL,
                        [ProviderKey] nvarchar(450) NOT NULL,
                        [ProviderDisplayName] nvarchar(max) NULL,
                        [UserId] nvarchar(450) NOT NULL,
                        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
                        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId])
                            REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserRoles' AND xtype='U')
                BEGIN
                    CREATE TABLE [AspNetUserRoles] (
                        [UserId] nvarchar(450) NOT NULL,
                        [RoleId] nvarchar(450) NOT NULL,
                        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
                        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId])
                            REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId])
                            REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserTokens' AND xtype='U')
                BEGIN
                    CREATE TABLE [AspNetUserTokens] (
                        [UserId] nvarchar(450) NOT NULL,
                        [LoginProvider] nvarchar(450) NOT NULL,
                        [Name] nvarchar(450) NOT NULL,
                        [Value] nvarchar(max) NULL,
                        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
                        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId])
                            REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='QuestionOptions' AND xtype='U')
                BEGIN
                    CREATE TABLE [QuestionOptions] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [QuestionId] int NOT NULL,
                        [Text] nvarchar(200) NOT NULL,
                        [NumericValue] int NOT NULL,
                        CONSTRAINT [PK_QuestionOptions] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_QuestionOptions_Questions_QuestionId] FOREIGN KEY ([QuestionId])
                            REFERENCES [Questions] ([Id]) ON DELETE CASCADE
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SurveyResponses' AND xtype='U')
                BEGIN
                    CREATE TABLE [SurveyResponses] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [UserId] nvarchar(450) NOT NULL,
                        [QuestionId] int NOT NULL,
                        [AnswerValue] int NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_SurveyResponses] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_SurveyResponses_AspNetUsers_UserId] FOREIGN KEY ([UserId])
                            REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_SurveyResponses_Questions_QuestionId] FOREIGN KEY ([QuestionId])
                            REFERENCES [Questions] ([Id]) ON DELETE CASCADE
                    );
                END
            ");

            // Indexes
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_AspNetRoleClaims_RoleId') CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='RoleNameIndex') CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_AspNetUserClaims_UserId') CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_AspNetUserLogins_UserId') CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_AspNetUserRoles_RoleId') CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='EmailIndex') CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_AspNetUsers_TrackId') CREATE INDEX [IX_AspNetUsers_TrackId] ON [AspNetUsers] ([TrackId]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='UserNameIndex') CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CategoryLabels_TrackId') CREATE INDEX [IX_CategoryLabels_TrackId] ON [CategoryLabels] ([TrackId]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_QuestionOptions_QuestionId') CREATE INDEX [IX_QuestionOptions_QuestionId] ON [QuestionOptions] ([QuestionId]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Questions_TrackId') CREATE INDEX [IX_Questions_TrackId] ON [Questions] ([TrackId]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_SurveyResponses_QuestionId') CREATE INDEX [IX_SurveyResponses_QuestionId] ON [SurveyResponses] ([QuestionId]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_SurveyResponses_UserId_QuestionId') CREATE UNIQUE INDEX [IX_SurveyResponses_UserId_QuestionId] ON [SurveyResponses] ([UserId], [QuestionId]);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID('SurveyResponses') IS NOT NULL DROP TABLE [SurveyResponses];");
            migrationBuilder.Sql(@"IF OBJECT_ID('QuestionOptions') IS NOT NULL DROP TABLE [QuestionOptions];");
            migrationBuilder.Sql(@"IF OBJECT_ID('CategoryLabels') IS NOT NULL DROP TABLE [CategoryLabels];");
            migrationBuilder.Sql(@"IF OBJECT_ID('AspNetUserTokens') IS NOT NULL DROP TABLE [AspNetUserTokens];");
            migrationBuilder.Sql(@"IF OBJECT_ID('AspNetUserRoles') IS NOT NULL DROP TABLE [AspNetUserRoles];");
            migrationBuilder.Sql(@"IF OBJECT_ID('AspNetUserLogins') IS NOT NULL DROP TABLE [AspNetUserLogins];");
            migrationBuilder.Sql(@"IF OBJECT_ID('AspNetUserClaims') IS NOT NULL DROP TABLE [AspNetUserClaims];");
            migrationBuilder.Sql(@"IF OBJECT_ID('Questions') IS NOT NULL DROP TABLE [Questions];");
            migrationBuilder.Sql(@"IF OBJECT_ID('AspNetUsers') IS NOT NULL DROP TABLE [AspNetUsers];");
            migrationBuilder.Sql(@"IF OBJECT_ID('AspNetRoleClaims') IS NOT NULL DROP TABLE [AspNetRoleClaims];");
            migrationBuilder.Sql(@"IF OBJECT_ID('AspNetRoles') IS NOT NULL DROP TABLE [AspNetRoles];");
            migrationBuilder.Sql(@"IF OBJECT_ID('Tracks') IS NOT NULL DROP TABLE [Tracks];");
        }
    }
}
