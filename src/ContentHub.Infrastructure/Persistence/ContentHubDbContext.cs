using ContentHub.Domain.Content;
using ContentHub.Domain.Users;
using ContentHub.Domain.Comments;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Infrastructure.Persistence
{
    public class ContentHubDbContext : DbContext
    {
        public ContentHubDbContext(DbContextOptions<ContentHubDbContext> options) : base(options) 
        {
        }

        public DbSet<ContentItem> ContentItems => Set<ContentItem>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentHubDbContext).Assembly);

            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                modelBuilder.Entity<ContentItem>()
                    .Property(x => x.RowVersion)
                    .IsRequired()
                    .ValueGeneratedNever();
            }
        }
    }
}
