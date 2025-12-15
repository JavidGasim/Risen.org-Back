using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Additional configurations can be added here if needed

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

            // Plan seed (stabil GUID-lərlə)
            var freeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var premiumId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var lifetimeId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            builder.Entity<Plan>().HasData(
                new Plan { Id = freeId, Code = PlanCode.Free, Name = "Free" },
                new Plan { Id = premiumId, Code = PlanCode.Premium, Name = "Premium" },
                new Plan { Id = lifetimeId, Code = PlanCode.Lifetime, Name = "Lifetime" }
            );
        }
    }
}
