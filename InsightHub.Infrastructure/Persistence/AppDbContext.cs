using InsightHub.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InsightHub.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<SurveyResponse> SurveyResponses { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<CategoryLabel> CategoryLabels { get; set; }
    public DbSet<JobOffer> JobOffers { get; set; }
    public DbSet<NewsArticle> NewsArticles { get; set; }
    public DbSet<InterviewQuestion> InterviewQuestions { get; set; }
    public DbSet<InterviewQoption> InterviewQuestionOptions { get; set; }
    public DbSet<QuizResult> QuizResults { get; set; }
    public DbSet<QuizResultTrack> QuizResultTracks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SurveyResponse>()
            .HasIndex(r => new { r.UserId, r.QuestionId })
            .IsUnique();

        builder.Entity<SurveyResponse>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.SurveyResponses)
            .HasForeignKey(r => r.UserId);

        builder.Entity<SurveyResponse>()
            .HasOne(r => r.Question)
            .WithMany(q => q.Responses)
            .HasForeignKey(r => r.QuestionId);

        builder.Entity<QuestionOption>()
            .HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId);

        builder.Entity<Question>()
            .HasOne(q => q.Track)
            .WithMany(t => t.Questions)
            .HasForeignKey(q => q.TrackId)
            .IsRequired(false);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Track)
            .WithMany()
            .HasForeignKey(u => u.TrackId)
            .IsRequired(false);

        builder.Entity<JobOffer>()
            .HasIndex(x => x.ExternalId)
            .IsUnique();

        builder.Entity<QuizResult>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.UserId);

        builder.Entity<QuizResult>()
            .HasMany(r => r.Tracks)
            .WithOne(t => t.QuizResult)
            .HasForeignKey(t => t.QuizResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
