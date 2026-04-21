using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Concretes;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using Risen.Contracts.Gamification;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Risen.Business.Tests
{
    public class XpServiceRevokeAndConcurrencyTests
    {
        private AppDbContext CreateSqliteContext(SqliteConnection conn)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conn)
                .Options;

            var ctx = new AppDbContext(options);
            ctx.Database.EnsureCreated();

            // seed league tier if not exists
            if (!ctx.LeagueTiers.Any())
            {
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
            }

            return ctx;
        }

        [Fact]
        public async Task Revoke_Creates_Compensating_Transaction_And_AdminAction()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();

            // award first
            using (var ctx = CreateSqliteContext(conn))
            {
                var statsService = new StatsService(ctx, null!);
                var xpService = new XpService(ctx, statsService);

                var adminId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                var req = new AwardXpRequest(
                    SourceType: XpSourceType.EventReward,
                    SourceKey: "evt:to-revoke",
                    BaseXp: 120,
                    DifficultyMultiplier: 1.0m,
                    TargetUserId: userId
                );

                var res = await xpService.AwardAsync(adminId, req, default);
                Assert.Equal(120, res.FinalXp);
            }

            // revoke
            using (var ctx = CreateSqliteContext(conn))
            {
                var statsService = new StatsService(ctx, null!);
                var xpService = new XpService(ctx, statsService);

                var adminId = Guid.NewGuid();
                var userId = ctx.XpTransactions.AsNoTracking().First().UserId;

                var revokeReq = new Risen.Contracts.Gamification.RevokeXpRequest(
                    TargetUserId: userId,
                    OriginalSourceKey: "evt:to-revoke",
                    Reason: "mistake"
                );

                var revokeRes = await xpService.RevokeAsync(adminId, revokeReq, default);

                // ensure compensating negative tx exists
                var txs = ctx.XpTransactions.AsNoTracking().Where(t => t.UserId == userId).ToList();
                Assert.Equal(2, txs.Count);
                Assert.Contains(txs, t => t.FinalXp < 0);

                // ensure admin action recorded
                var adminActions = ctx.AdminActions.AsNoTracking().Where(a => a.TargetUserId == userId).ToList();
                Assert.Single(adminActions);
                Assert.Equal(adminId, adminActions[0].AdminId);
            }
        }

        [Fact]
        public async Task Concurrent_Awards_Only_Create_One_Transaction()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();

            // create schema and seed
            using (var seedCtx = CreateSqliteContext(conn)) { }

            var key = "concurrent:evt:1";
            var userId = Guid.NewGuid();
            var adminId1 = Guid.NewGuid();
            var adminId2 = Guid.NewGuid();

            // run two parallel award tasks using separate contexts
            var t1 = Task.Run(async () =>
            {
                using var ctx1 = CreateSqliteContext(conn);
                var svc1 = new XpService(ctx1, new StatsService(ctx1, null!));
                var req = new AwardXpRequest(XpSourceType.EventReward, key, 50, 1.0m, userId);
                return await svc1.AwardAsync(adminId1, req, default);
            });

            var t2 = Task.Run(async () =>
            {
                using var ctx2 = CreateSqliteContext(conn);
                var svc2 = new XpService(ctx2, new StatsService(ctx2, null!));
                var req = new AwardXpRequest(XpSourceType.EventReward, key, 50, 1.0m, userId);
                return await svc2.AwardAsync(adminId2, req, default);
            });

            await Task.WhenAll(t1, t2);

            using var verifyCtx = CreateSqliteContext(conn);
            var txCount = await verifyCtx.XpTransactions.CountAsync();
            Assert.Equal(1, txCount);

            var stats = await verifyCtx.UserStats.FirstOrDefaultAsync(s => s.UserId == userId);
            Assert.NotNull(stats);
            Assert.Equal(50, stats.TotalXp);
        }
    }
}
