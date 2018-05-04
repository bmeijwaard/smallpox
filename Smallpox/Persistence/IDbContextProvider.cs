using Smallpox.Messages;
using System;
using System.Threading.Tasks;

namespace Smallpox.Persistence
{
    public interface IDbContextProvider
    {
        IDbContext Context { get; }
        Task<IServiceResponse> ExecuteTransactionAsync<T>(Func<IDbContext, Task<T>> contextFunc) where T : IServiceResponse;
    }
}