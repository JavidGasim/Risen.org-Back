using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Risen.DataAccess.Data
{
    public class AppDbContext : IdentityDbContext<CustomIdentityUser, CustomIdentityRole, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<University> Universities => Set<University>();
        public DbSet<LeagueTier> LeagueTiers => Set<LeagueTier>();
        public DbSet<UserStats> UserStats => Set<UserStats>();
        public DbSet<XpTransaction> XpTransactions => Set<XpTransaction>();
        public DbSet<XpTransactionArchive> XpTransactionArchives => Set<XpTransactionArchive>();
        public DbSet<UserLeagueHistory> UserLeagueHistories => Set<UserLeagueHistory>();
        public DbSet<Quest> Quests => Set<Quest>();
        public DbSet<QuestOption> QuestOptions => Set<QuestOption>();
        public DbSet<QuestAttempt> QuestAttempts => Set<QuestAttempt>();
        public DbSet<Risen.Entities.Entities.AdminAction> AdminActions => Set<Risen.Entities.Entities.AdminAction>();
        public DbSet<Subject> Subjects => Set<Subject>();



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // -------------------------
            // Identity User
            // -------------------------
            builder.Entity<CustomIdentityUser>(e =>
            {
                e.Property(x => x.FirstName).HasMaxLength(64).IsRequired();
                e.Property(x => x.LastName).HasMaxLength(64).IsRequired();
                e.Property(x => x.FullName).HasMaxLength(128).IsRequired();

                e.HasIndex(x => x.LastOnlineAtUtc);

                // User -> University FK
                e.HasOne(x => x.University)
                 .WithMany()
                 .HasForeignKey(x => x.UniversityId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // -------------------------
            // Plan
            // -------------------------
            builder.Entity<Plan>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Code).IsUnique();
                e.Property(x => x.Name).HasMaxLength(64).IsRequired();
            });

            builder.Entity<UserSubscription>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Plan)
                 .WithMany()
                 .HasForeignKey(x => x.PlanId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.UserId, x.IsActive });
            });

            builder.Entity<RefreshToken>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.TokenHash).IsUnique();
                e.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            });

            // -------------------------
            // University
            // -------------------------
            builder.Entity<University>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Name).HasMaxLength(256).IsRequired();

                e.Property(x => x.NormalizedKey).HasMaxLength(300).IsRequired();
                e.HasIndex(x => x.NormalizedKey).IsUnique();

                // Country NULL ola bilər dedin – REQUIRED ETMİRİK
                e.Property(x => x.Country).HasMaxLength(128);
                e.Property(x => x.StateProvince).HasMaxLength(128);
                e.Property(x => x.PrimaryDomain).HasMaxLength(200);
                e.Property(x => x.PrimaryWebPage).HasMaxLength(500);
            });

            // -------------------------
            // LeagueTier
            // -------------------------
            builder.Entity<LeagueTier>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Code).IsUnique();
                e.HasIndex(x => x.SortOrder).IsUnique();
                e.Property(x => x.Name).HasMaxLength(64).IsRequired();
                e.Property(x => x.Weight).IsRequired();  // ← əlavə et
            });

            // -------------------------
            // UserStats
            // -------------------------
            builder.Entity<UserStats>(e =>
            {
                e.HasKey(x => x.UserId);

                e.HasOne(x => x.User)
                 .WithOne(u => u.Stats)
                 .HasForeignKey<UserStats>(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.CurrentLeagueTier)
                 .WithMany()
                 .HasForeignKey(x => x.CurrentLeagueTierId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.TotalXp);

                // streak date only
                e.Property(x => x.LastStreakDateUtc).HasColumnType("date");
                e.Property(x => x.RisenScore)
                 .HasPrecision(10, 2)
                 .HasDefaultValue(0);
            });

            // -------------------------
            // XpTransaction
            // -------------------------
            builder.Entity<XpTransaction>(e =>
            {
                e.HasKey(x => x.Id);

                // Unique idempotency key per user and source type
                e.HasIndex(x => new { x.UserId, x.SourceType, x.SourceKey }).IsUnique();

                e.Property(x => x.SourceKey).HasMaxLength(128).IsRequired();

                e.Property(x => x.DifficultyMultiplier).HasPrecision(6, 2);

                e.Property(x => x.CreatedAtUtc).IsRequired();
                e.Property(x => x.AdminReason).HasMaxLength(512);
                e.HasIndex(x => x.AdminId);
            });

            builder.Entity<XpTransactionArchive>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.SourceKey).HasMaxLength(128).IsRequired();
                e.Property(x => x.DifficultyMultiplier).HasPrecision(6, 2);
                e.Property(x => x.ArchivedAtUtc).IsRequired();
                e.HasIndex(x => x.UserId);
                e.HasIndex(x => x.AdminId);
            });

            builder.Entity<Risen.Entities.Entities.AdminAction>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ActionType).HasMaxLength(64).IsRequired();
                e.Property(x => x.Details).HasMaxLength(2000);
                e.Property(x => x.CreatedAtUtc).IsRequired();
                e.HasIndex(x => x.AdminId);
                e.HasIndex(x => x.TargetUserId);
            });

            builder.Entity<Risen.Entities.Entities.Subject>(e =>
            {
                e.HasKey(x => x.Code);
                e.Property(x => x.Code).HasMaxLength(64).IsRequired();
                e.Property(x => x.Name).HasMaxLength(128).IsRequired();
                e.Property(x => x.Description).HasMaxLength(2000);
                e.Property(x => x.CreatedAtUtc).IsRequired();
                e.HasIndex(x => x.IsActive);
            });

            // -------------------------
            // League History
            // -------------------------
            builder.Entity<UserLeagueHistory>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.UserId, x.ChangedAtUtc });

                e.HasOne(x => x.FromTier).WithMany().HasForeignKey(x => x.FromTierId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.ToTier).WithMany().HasForeignKey(x => x.ToTierId).OnDelete(DeleteBehavior.Restrict);
            });

            // -------------------------
            // Quest
            // -------------------------
            builder.Entity<Quest>(b =>
            {
                b.HasKey(x => x.Id);

                // Əgər Quest-də Title varsa:
                b.Property(x => x.Title).HasMaxLength(256).IsRequired();

                // Səndə var deyə saxlayıram:
                b.Property(x => x.QuestionText).HasMaxLength(2000).IsRequired();

                b.HasMany(x => x.Options)
                 .WithOne(x => x.Quest)
                 .HasForeignKey(x => x.QuestId)
                 .OnDelete(DeleteBehavior.Cascade);

                // CorrectOptionIndex (0..4)
                b.Property(x => x.CorrectOptionIndex).IsRequired();
            });

            builder.Entity<QuestOption>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.Text).IsRequired().HasMaxLength(1000);
                b.Property(x => x.Index).IsRequired();

                // hər quest üçün 0..4 unikaldır
                b.HasIndex(x => new { x.QuestId, x.Index }).IsUnique();
            });

            builder.Entity<QuestAttempt>(e =>
            {
                e.HasKey(x => x.Id);

                // Submit zamanı həmişə yazılır
                e.Property(x => x.CompletedAtUtc).IsRequired();

                // bu submit-də qazandığı XP (quest + streak bonus ola bilər)
                e.Property(x => x.AwardedXp).HasDefaultValue(0);

                e.HasOne(x => x.Quest)
                 .WithMany()
                 .HasForeignKey(x => x.QuestId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.SelectedOption)
                 .WithMany()
                 .HasForeignKey(x => x.SelectedOptionId)
                 .OnDelete(DeleteBehavior.Restrict);

                // “Tamamlanma” yalnız CompletedDateUtc NULL deyilsə sayılır.
                // Eyni gün eyni quest 1 dəfə tamamlanıb qeyd olunsun (correct completion).
                e.HasIndex(x => new { x.UserId, x.QuestId, x.CompletedDateUtc })
                 .HasFilter("[CompletedDateUtc] IS NOT NULL")
                 .IsUnique();
            });

            // -------------------------
            // Seeds
            // -------------------------

            var freeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var premiumId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var lifetimeId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            builder.Entity<Plan>().HasData(
                new Plan { Id = freeId, Code = PlanCode.Free, Name = "Free" },
                new Plan { Id = premiumId, Code = PlanCode.Premium, Name = "Premium" },
                new Plan { Id = lifetimeId, Code = PlanCode.Lifetime, Name = "Lifetime" }
            );

            var rookieId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var bronzeId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var silverId = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var goldId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            var platinumId = Guid.Parse("88888888-8888-8888-8888-888888888888");
            var diamondId = Guid.Parse("99999999-9999-9999-9999-999999999999");
            var masterId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var legendId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

            builder.Entity<LeagueTier>().HasData(
    new LeagueTier { Id = rookieId, Code = LeagueCode.Rookie, Name = "Rookie", MinXp = 0, MaxXp = 999, SortOrder = 1, Weight = 0 },
    new LeagueTier { Id = bronzeId, Code = LeagueCode.Bronze, Name = "Bronze", MinXp = 1000, MaxXp = 2499, SortOrder = 2, Weight = 10 },
    new LeagueTier { Id = silverId, Code = LeagueCode.Silver, Name = "Silver", MinXp = 2500, MaxXp = 4999, SortOrder = 3, Weight = 20 },
    new LeagueTier { Id = goldId, Code = LeagueCode.Gold, Name = "Gold", MinXp = 5000, MaxXp = 9999, SortOrder = 4, Weight = 35 },
    new LeagueTier { Id = platinumId, Code = LeagueCode.Platinum, Name = "Platinum", MinXp = 10000, MaxXp = 19999, SortOrder = 5, Weight = 50 },
    new LeagueTier { Id = diamondId, Code = LeagueCode.Diamond, Name = "Diamond", MinXp = 20000, MaxXp = 39999, SortOrder = 6, Weight = 70 },
    new LeagueTier { Id = masterId, Code = LeagueCode.Master, Name = "Master", MinXp = 40000, MaxXp = 79999, SortOrder = 7, Weight = 90 },
    new LeagueTier { Id = legendId, Code = LeagueCode.Legend, Name = "Legend", MinXp = 80000, MaxXp = null, SortOrder = 8, Weight = 120 }
);
        }
    }
}
