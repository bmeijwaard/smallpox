using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Smallpox.Entities;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Smallpox.Persistence
{
    public class SqlContext : IdentityDbContext<User, Role, Guid>, IDbContext
    {
        public SqlContext(DbContextOptions options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //override for aspnetuser tables
            builder.CreateCustomSecurityModel();

            //set roles for each user
            builder.Entity<UserRole>()
                .HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }

        public IDbContextTransaction BeginTransaction()
        {
            return Database.BeginTransaction();
        }

        public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return Database.BeginTransaction(isolationLevel);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await Database.BeginTransactionAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync(CancellationToken.None);
        }
    }
}
