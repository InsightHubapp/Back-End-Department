using InsightHub.Domain.Entities;
using InsightHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InsightHub.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        if (!await db.Tracks.AnyAsync())
        {
            var itJobs = new CategoryLabel { Name = "IT Jobs" };
            var creativeJobs = new CategoryLabel { Name = "Creative & Design Jobs" };
            var engineeringJobs = new CategoryLabel { Name = "Engineering Jobs" };
            var scientificJobs = new CategoryLabel { Name = "Scientific & QA Jobs" };

            var tracks = new List<Track>
            {
                new Track { Id = 2, Name = "Backend Dev", Description = "Server-side logic, database management, and API development.", RequiredSkills = "C# (.NET), SQL Server, Entity Framework, RESTful APIs, JWT", CategoryLabels = new List<CategoryLabel> { itJobs } },
                new Track { Id = 3, Name = "Frontend Dev", Description = "Creating the visual elements and interactive user interface.", RequiredSkills = "HTML5, CSS3, JavaScript (ES6+), React.js/Angular", CategoryLabels = new List<CategoryLabel> { itJobs, creativeJobs } },
                new Track { Id = 4, Name = "Mobile Dev", Description = "Develops applications for iOS and Android platforms.", RequiredSkills = "Dart (Flutter), State Management, Firebase", CategoryLabels = new List<CategoryLabel> { itJobs } },
                new Track { Id = 5, Name = "Game Dev", Description = "Designing game mechanics and interactive environments.", RequiredSkills = "C#, Unity Engine, Game Physics, Basic 3D Math", CategoryLabels = new List<CategoryLabel> { itJobs, creativeJobs } },
                new Track { Id = 6, Name = "Cybersecurity", Description = "Protects systems, networks, and data from digital attacks.", RequiredSkills = "Networking (TCP/IP), Linux, OWASP Top 10", CategoryLabels = new List<CategoryLabel> { itJobs } },
                new Track { Id = 7, Name = "Embedded", Description = "Programming microcontrollers and hardware devices.", RequiredSkills = "Embedded C/C++, ARM Cortex, UART/I2C/SPI", CategoryLabels = new List<CategoryLabel> { itJobs, engineeringJobs } },
                new Track { Id = 8, Name = "AI/ML", Description = "Builds systems that learn from data to make predictions.", RequiredSkills = "Python, Statistics, Linear Algebra, TensorFlow/PyTorch", CategoryLabels = new List<CategoryLabel> { itJobs, scientificJobs } },
                new Track { Id = 9, Name = "QA/Testing", Description = "Ensures software quality through manual and automated testing.", RequiredSkills = "Manual Testing, Selenium, Bug Reporting, SQL", CategoryLabels = new List<CategoryLabel> { itJobs, scientificJobs } },
                new Track { Id = 10, Name = "Data Analysis", Description = "Analyzes raw data to uncover trends and insights.", RequiredSkills = "Advanced Excel, SQL, Power BI/Tableau, Python", CategoryLabels = new List<CategoryLabel> { itJobs, scientificJobs } }
            };

            using var trx = await db.Database.BeginTransactionAsync();
            try
            {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Tracks ON");
                await db.Tracks.AddRangeAsync(tracks);
                await db.SaveChangesAsync();
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Tracks OFF");
                await trx.CommitAsync();
            }
            catch
            {
                await trx.RollbackAsync();
                throw;
            }
        }

        var existingIds = await db.Questions
            .AsNoTracking()
            .Where(q => q.Id >= 101 && q.Id <= 160)
            .Select(q => q.Id)
            .ToListAsync();

        var existingSet = existingIds.ToHashSet();

        var questions = BuildQuestions()
            .Where(q => !existingSet.Contains(q.Id))
            .ToList();

        if (questions.Any())
        {
            using var trx = await db.Database.BeginTransactionAsync();
            try
            {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Questions ON");
                await db.Questions.AddRangeAsync(questions);
                await db.SaveChangesAsync();
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Questions OFF");
                await trx.CommitAsync();
            }
            catch
            {
                await trx.RollbackAsync();
                throw;
            }
        }
    }

    public static async Task SeedEmployeeResponsesAsync(AppDbContext db)
    {
        var employees = await db.Users.Where(u => u.IsEmployed).ToListAsync();
        if (!employees.Any()) return;

        var employeeIds = employees.Select(u => u.Id).ToList();
        var insightQuestionIds = Enumerable.Range(101, 20).ToList();

        var existingResponses = await db.SurveyResponses
            .Where(r => employeeIds.Contains(r.UserId) && insightQuestionIds.Contains(r.QuestionId))
            .ToListAsync();

        if (existingResponses.Any())
        {
            db.SurveyResponses.RemoveRange(existingResponses);
            await db.SaveChangesAsync();
        }

        var random = new Random();
        var responsesToSeed = new List<SurveyResponse>();

        foreach (var employee in employees)
        {
            foreach (var qId in insightQuestionIds)
            {
                int val;
                if (qId >= 113 && qId <= 115)
                    val = random.Next(1, 4);
                else
                    val = random.Next(3, 6);

                responsesToSeed.Add(new SurveyResponse
                {
                    UserId = employee.Id,
                    QuestionId = qId,
                    AnswerValue = val
                });
            }
        }

        if (responsesToSeed.Any())
        {
            await db.SurveyResponses.AddRangeAsync(responsesToSeed);
            await db.SaveChangesAsync();
        }

        if (!await db.QuestionOptions.AnyAsync())
        {
            var options = new List<QuestionOption>
            {
                new QuestionOption { QuestionId = 113, Text = "Remote", NumericValue = 1 },
                new QuestionOption { QuestionId = 113, Text = "Office", NumericValue = 2 },
                new QuestionOption { QuestionId = 113, Text = "Hybrid", NumericValue = 3 },

                new QuestionOption { QuestionId = 114, Text = "Startup", NumericValue = 1 },
                new QuestionOption { QuestionId = 114, Text = "Mid-size", NumericValue = 2 },
                new QuestionOption { QuestionId = 114, Text = "Corporate", NumericValue = 3 },

                new QuestionOption { QuestionId = 115, Text = "Technical", NumericValue = 1 },
                new QuestionOption { QuestionId = 115, Text = "Managerial", NumericValue = 2 },
                new QuestionOption { QuestionId = 115, Text = "Balanced", NumericValue = 3 }
            };

            await db.QuestionOptions.AddRangeAsync(options);
            await db.SaveChangesAsync();
        }
    }

    private static List<Question> BuildQuestions()
    {
        var result = new List<Question>();
        int order = 1;

        void AddEmployed(int id, string text) =>
            result.Add(new Question { Id = id, Text = text, Type = QuestionType.Scale, AppliesTo = TargetGroup.Employed, Order = order++, MaxValue = 5, IsCareerQuiz = false });

        void AddShared(int id, string text, int max = 5) =>
            result.Add(new Question { Id = id, Text = text, Type = QuestionType.Scale, AppliesTo = TargetGroup.Both, Order = order++, MaxValue = 5, IsCareerQuiz = false });

        void AddQuiz(int id, string text, int trackId) =>
            result.Add(new Question { Id = id, Text = text, Type = QuestionType.YesNo, AppliesTo = TargetGroup.Unemployed, IsCareerQuiz = true, Order = order++, TrackId = trackId });

        void AddChoice(int id, string text, int maxValue) =>
            result.Add(new Question { Id = id, Text = text, Type = QuestionType.MultiChoice, AppliesTo = TargetGroup.Both, IsCareerQuiz = false, Order = order++, MaxValue = maxValue, TrackId = null });

        // [101-110] Behavioral
        AddEmployed(101, "How consistently do you finish tasks on time?");
        AddEmployed(102, "How well do you handle feedback and adapt?");
        AddEmployed(103, "How comfortable are you working in a team?");
        AddEmployed(104, "How strong is your problem-solving approach?");
        AddEmployed(105, "How proactive are you in learning new skills?");
        AddEmployed(106, "How clearly do you communicate technical ideas?");
        AddEmployed(107, "How well do you prioritize tasks under pressure?");
        AddEmployed(108, "How often do you take ownership of outcomes?");
        AddEmployed(109, "How effectively do you collaborate across roles?");
        AddEmployed(110, "How resilient are you when facing setbacks?");

        // [111-120] Shared
        AddShared(111, "How aligned are you with your current field?");
        AddShared(112, "How would you rate your experience level?", 3);
        AddChoice(113, "Preferred work environment.", 3);
        AddChoice(114, "Preferred company size.", 3);
        AddChoice(115, "Preferred role style.", 3);
        AddShared(116, "How do you rate your technical level?");
        AddShared(117, "How do you rate your soft skills?");
        AddShared(118, "How satisfied are you with your current salary?");
        AddShared(119, "How satisfied are you with your work-life balance?");
        AddShared(120, "How satisfied are you with your career direction?");

        // [121-160] Career Quiz
        AddQuiz(121, "Do you enjoy organizing information in a clear system?", 2);
        AddQuiz(122, "Do you care more about how things work than how they look?", 2);
        AddQuiz(123, "Do you like solving logic problems?", 2);
        AddQuiz(124, "Do you enjoy connecting different parts of a system together?", 2);
        AddQuiz(125, "Do you like designing?", 3);
        AddQuiz(126, "Do you enjoy choosing colors, layouts, and styles?", 3);
        AddQuiz(127, "Are you interested in creating buttons and pages people interact with?", 3);
        AddQuiz(128, "Do you enjoy adding animations to your projects?", 3);
        AddQuiz(129, "Do you enjoy mathematics?", 8);
        AddQuiz(130, "Do you like data and patterns?", 8);
        AddQuiz(131, "Are you curious about how machines can think or learn?", 8);
        AddQuiz(132, "Do you enjoy solving complex problems?", 8);
        AddQuiz(133, "Do you care about privacy?", 6);
        AddQuiz(134, "Do you enjoy finding weak points in systems?", 6);
        AddQuiz(135, "Do you care about information protection?", 6);
        AddQuiz(136, "Do you stay calm in urgent situations?", 6);
        AddQuiz(137, "Do you like making simple and practical products?", 4);
        AddQuiz(138, "Do you enjoy making tools people use every day?", 4);
        AddQuiz(139, "Are you interested in how apps are built on phones?", 4);
        AddQuiz(140, "Do you want to build a daily-use app?", 4);
        AddQuiz(141, "Do you like creating fun experiences for people?", 5);
        AddQuiz(142, "Do you enjoy imagination and world-building?", 5);
        AddQuiz(143, "Do you enjoy designing challenges for others?", 5);
        AddQuiz(144, "Do you like creating characters or stories?", 5);
        AddQuiz(145, "Do you enjoy detecting bugs?", 9);
        AddQuiz(146, "Do you like ensuring software works correctly?", 9);
        AddQuiz(147, "Do you enjoy creating test cases and scenarios?", 9);
        AddQuiz(148, "Do you like analyzing software performance?", 9);
        AddQuiz(149, "Do you care about clear and accurate information?", 10);
        AddQuiz(150, "Do you like finding patterns in data?", 10);
        AddQuiz(151, "Do you enjoy creating visualizations to represent data?", 10);
        AddQuiz(152, "Do you like analyzing data to make decisions?", 10);
        AddQuiz(153, "Are you curious about how software interacts with hardware?", 7);
        AddQuiz(154, "Do you like making devices work using programming?", 7);
        AddQuiz(155, "Are you interested in how everyday devices become smart?", 7);
        AddQuiz(156, "Do you prefer working on things that combine electricity and programming?", 7);
        AddQuiz(157, "Do you like working with data and records?", 2);
        AddQuiz(158, "Do you enjoy organizing content and elements in pages?", 3);
        AddQuiz(159, "Do you like systems that can predict outcomes?", 8);
        AddQuiz(160, "Do you enjoy improving software quality?", 9);

        return result;
    }
}
