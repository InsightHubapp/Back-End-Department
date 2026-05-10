using Bogus;
using InsightHub.Domain.Entities;
using InsightHub.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InsightHub.Infrastructure.Persistence;

public static class DummyDataSeeder
{
    private const int TargetUserCount = 2000;
    private const string UserNamePrefix = "market_emp_";

    private static readonly int[] TrackIds = [2, 3, 4, 5, 6, 7, 8, 9, 10];
    private static readonly int[] AllEmployeeQuestionIds = Enumerable.Range(101, 20).ToArray();

    private static readonly string[] Collages =
    [
        "Faculty of Engineering",
        "Faculty of Computers and AI",
        "Faculty of Information Systems",
        "Faculty of Science"
    ];

    public static async Task SeedMarketDataAsync(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(userManager);

        var availableQuestionIds = await context.Questions
            .AsNoTracking()
            .Where(q => AllEmployeeQuestionIds.Contains(q.Id))
            .Select(q => q.Id)
            .ToListAsync();

        if (availableQuestionIds.Count != AllEmployeeQuestionIds.Length)
        {
            throw new InvalidOperationException("Employee questions (101-120) must exist before seeding dummy data.");
        }

        var existingSeededCount = await context.Users
            .AsNoTracking()
            .CountAsync(u => u.UserName != null && u.UserName.StartsWith(UserNamePrefix));

        var usersToCreate = Math.Max(0, TargetUserCount - existingSeededCount);
        if (usersToCreate <= 0) return;

        var faker = new Faker("en");
        var createdUsers = new List<ApplicationUser>();

        for (var i = 0; i < usersToCreate; i++)
        {
            var firstName = faker.Name.FirstName();
            var lastName = faker.Name.LastName();
            var serial = Guid.NewGuid().ToString("N")[..8];
            var userName = $"{UserNamePrefix}{serial}";

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = $"{userName}@insight-dummy.com",
                FirstName = firstName,
                LastName = lastName,
                Gender = faker.PickRandom<Gender>(),
                Collage = faker.PickRandom(Collages),
                IsEmployed = true,
                YearsExperience = faker.Random.Int(1, 10),
                TrackId = faker.PickRandom(TrackIds),
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, "Emp@12345");
            if (createResult.Succeeded)
            {
                createdUsers.Add(user);
            }
        }

        var responses = new List<SurveyResponse>();

        foreach (var user in createdUsers)
        {
            foreach (var questionId in AllEmployeeQuestionIds)
            {
                int answerValue;

                if (questionId >= 101 && questionId <= 111)
                {
                    answerValue = faker.Random.Int(3, 5);
                }
                else if (questionId >= 112 && questionId <= 115)
                {
                    answerValue = faker.Random.Int(1, 3);
                }
                else
                {
                    answerValue = faker.Random.Int(1, 5);
                }

                responses.Add(new SurveyResponse
                {
                    UserId = user.Id,
                    QuestionId = questionId,
                    AnswerValue = answerValue,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var batches = responses.Chunk(1000);
        foreach (var batch in batches)
        {
            await context.SurveyResponses.AddRangeAsync(batch);
            await context.SaveChangesAsync();
        }
    }
}
