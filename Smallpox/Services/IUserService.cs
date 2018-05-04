using Smallpox.Entities;
using Smallpox.Entities.Information;
using Smallpox.Entities.Types;
using Smallpox.Messages;
using System;
using System.Threading.Tasks;

namespace Smallpox.Services
{
    public interface IUserService
    {
        Task<EntityResponse<User>> GetAsync(Guid id);
        Task<EntityResponse<User>> CreateAdminAsync(User user, Company company, string password);
        Task<EntityResponse<User>> CreateAsync(User user, Roles[] roles, Address address, Campus campus, Company company, Patient patient, Personal personal, string password);
    }
}