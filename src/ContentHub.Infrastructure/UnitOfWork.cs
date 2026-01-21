using ContentHub.Application.Common.Interfaces;
using ContentHub.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ContentHubDbContext _dbContext;

        public UnitOfWork(ContentHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CommitAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
