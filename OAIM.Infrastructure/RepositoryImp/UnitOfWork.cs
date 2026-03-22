using Microsoft.EntityFrameworkCore.Storage;
using OAIM.Domain.Interfaces;
using OAIM.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Infrastructure.RepositoryImp
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync()
       => await _context.SaveChangesAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync()
            => await _context.Database.BeginTransactionAsync();

        public void Dispose()
            => _context.Dispose();
    }
}
