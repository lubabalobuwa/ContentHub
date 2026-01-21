using ContentHub.Domain.Content;
using ContentHub.Domain.Users;
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
            Database.EnsureCreated();
        }

        public DbSet<ContentItem> ContentItems => Set<ContentItem>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentHubDbContext).Assembly);
        }
    }
}
