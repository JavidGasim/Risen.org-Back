using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Options;
using Risen.Business.Services.Concretes;
using Risen.Contracts.Gamification;
using Risen.Contracts.Quests;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Risen.Business.Tests
{
    public class QuestServiceTests
    {
        private AppDbContext CreateSqliteContext(SqliteConnection conn)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conn)
                .Options;

            var ctx = new AppDbContext(options);
            ctx.Database.EnsureCreated();

            // seed league tier
            ctx.LeagueTiers.Add(new LeagueTier
            {
                Id = Guid.NewGuid(),
                Code = LeagueCode.Rookie,
                Name = "Rookie",
                MinXp = 0,
                MaxXp = 999,
                SortOrder = 1,
                Weight = 0
            });

            ctx.SaveChanges();
            return ctx;
        }

        private class DummyEntitlement : Risen.Business.Services.Abstracts.IQuestEntitlementService
        {
            public Task<(bool isPremium, string? plan, int dailyLimit, bool advancedAllowed)> GetQuestPolicyAsync(Guid userId, CancellationToken ct)
            {
                return Task.FromResult((false, (string?)null, 10, false));
            }
        }

        [Fact]
        public async Task Submit_Correct_Answers_Award_XP_And_Streak()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();

            using var ctx = CreateSqliteContext(conn);

            var statsService = new StatsService(ctx, null!);
            var xpService = new XpService(ctx, statsService);

            var ent = new DummyEntitlement();
            var opt = Microsoft.Extensions.Options.Options.Create(new QuestPolicyOptions
            {
                AdvancedMultiplier = 2.0m,
                IntermediateMultiplier = 1.5m,
                NormalMultiplier = 1.0m,
                StreakBonusXp = 10
            });

            var questService = new QuestService(ctx, ent, xpService, opt);

            var userId = Guid.NewGuid();

            // create a quest with 5 options
            var quest = new Quest
            {
                Id = Guid.NewGuid(),
                Title = "T1",
                QuestionText = "Q?",
                BaseXp = 50,
                Difficulty = QuestDifficulty.Normal,
                IsActive = true,
                IsPremiumOnly = false,
                CorrectOptionIndex = 2
            };

            ctx.Quests.Add(quest);
            for (int i = 0; i < 5; i++)
            {
                ctx.QuestOptions.Add(new QuestOption
                {
                    Id = Guid.NewGuid(),
                    QuestId = quest.Id,
                    Index = i,
                    Text = "opt" + i
                });
            }
            ctx.SaveChanges();

            var req = new SubmitQuestAnswerRequest(QuestId: quest.Id, SelectedIndex: 2);

            var res = await questService.SubmitAsync(userId, req, default);

            Assert.True(res.IsCorrect);
            Assert.Equal(1, res.CurrentStreak);
            Assert.Equal(1, res.LongestStreak);
            Assert.Equal(2, await ctx.QuestAttempts.CountAsync()); // one for attempt, one XpTransaction? Note: QuestAttempt only
        }
    }
}
