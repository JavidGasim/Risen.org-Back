using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Concretes;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using Risen.Contracts.Gamification;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Risen.Business.Tests
{
    public class XpServiceTests
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var ctx = new AppDbContext(options);

            // seed a minimal LeagueTier
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

        [Fact]
        public async Task Award_Is_Idempotent_And_Admin_Audit_Saved()
        {
            var ctx = CreateContext("xp_test_db");

            var statsService = new StatsService(ctx, null!);
            var xpService = new XpService(ctx, statsService);

            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var req = new AwardXpRequest(
                SourceType: XpSourceType.EventReward,
                SourceKey: "evt:abc123",
                BaseXp: 100,
                DifficultyMultiplier: 1.0m,
                TargetUserId: userId
            );

            // admin awards once
            var res1 = await xpService.AwardAsync(adminId, req, default);

            // admin awards again with same source key — should be idempotent
            var res2 = await xpService.AwardAsync(adminId, req, default);

            Assert.Equal(res1.FinalXp, res2.FinalXp);
            Assert.Equal(res1.NewTotalXp, res2.NewTotalXp);

            // check only one transaction exists
            var txCount = await ctx.XpTransactions.CountAsync();
            Assert.Equal(1, txCount);

            // admin action recorded
            var adminActions = await ctx.AdminActions.ToListAsync();
            Assert.Single(adminActions);
            Assert.Equal(adminId, adminActions[0].AdminId);
            Assert.Equal(userId, adminActions[0].TargetUserId);
        }
    }
}
