using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Smallpox.Config.Settings;
using Smallpox.Messages;
using System;
using System.Threading.Tasks;

namespace Smallpox.Persistence
{
    public class DbContextProvider : IDbContextProvider
    {
        private readonly string _connectionString;

        public DbContextProvider(Func<SqlContext> context, IOptions<ConnectionStrings> connectionStrings)
        {
            Context = context();
            _connectionString = connectionStrings.Value.DefaultConnection;
        }

        public IDbContext Context { get; }

        public async Task<IServiceResponse> ExecuteTransactionAsync<T>(Func<IDbContext, Task<T>> contextFunc) where T : IServiceResponse
        {
            try
            {
                T result = default(T);
                using (var contextTransaction = await _context.BeginTransactionAsync())
                {
                    try
                    {
                        result = await contextFunc(_context);
                        contextTransaction.Commit();

                        return result;
                    }
                    catch (Exception e)
                    {
                        // we catch this exception to suppress (known) optimistic concurrency issue's
                        if (e.Message.Contains("Database operation expected to affect 1 row(s) but actually affected 0 row(s)."))
                        {
                            return result;
                        }
                        contextTransaction.Rollback();
                        throw e;
                    }
                }
            }
            finally
            {
                _context.Dispose();
            }
        }

        private IDbContext _context
        {
            get
            {
                return InitializeContext();
            }
        }

        private IDbContext InitializeContext()
        {
            var builder = new DbContextOptionsBuilder<SqlContext>();
            builder.UseSqlServer(_connectionString);
            return new SqlContext(builder.Options);
        }
    }
}
