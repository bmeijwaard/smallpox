using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Smallpox.Entities;
using Smallpox.Entities.Information;
using Smallpox.Entities.Types;
using Smallpox.Helpers.Identity;
using Smallpox.Messages;
using Smallpox.Persistence;
using Smallpox.Services.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Smallpox.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContextProvider _contextProvider;
        private readonly IUserManager _userManager;
        private readonly HttpContext _context;
        private readonly PasswordHasher _passwordHasher;

        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public UserService(IHttpContextAccessor context, IDbContextProvider contextProvider, IUserManager userManager)
        {
            _contextProvider = contextProvider;
            _userManager = userManager;
            _context = context.HttpContext;
            _passwordHasher = new PasswordHasher();
        }

        protected internal IList<IPasswordValidator<User>> PasswordValidators { get; } = new List<IPasswordValidator<User>>() as IList<IPasswordValidator<User>>;
        protected internal IList<IUserValidator<User>> UserValidators { get; } = new List<IUserValidator<User>>() as IList<IUserValidator<User>>;
        protected CancellationToken CancellationToken
        {
            get
            {
                var context = _context;
                return context?.RequestAborted ?? CancellationToken.None;
            }
        }

        public async Task<EntityResponse<User>> GetAsync(Guid id)
        {
            var user = await _contextProvider.Context.Users
                .Include(u => u.UserInformation)
                    .ThenInclude(ui => ui.Address)
                .Include(u => u.UserInformation)
                    .ThenInclude(ui => ui.Campus)
                .Include(u => u.UserInformation)
                    .ThenInclude(ui => ui.Company)
                .Include(u => u.UserInformation)
                    .ThenInclude(ui => ui.Personal)
                .Include(u => u.UserInformation)
                    .ThenInclude(ui => ui.Patient)
                .FirstOrDefaultAsync(u => u.Id == id);

            return new EntityResponse<User>(user);
        }

        public async Task<EntityResponse<User>> CreateAdminAsync(User user, Company company, string password)
        {
            return await CreateAsync(user, new Roles[1] { Roles.Administrator }, null, null, company, null, null, password);
        }

        public async Task<EntityResponse<User>> CreateAsync(User user, Roles[] roles, Address address, Campus campus, Company company, Patient patient, Personal personal, string password)
        {
            if (await _contextProvider.Context.Users.AnyAsync(u => u.NormalizedUserName == NormalizeKey(user.UserName)))
            {
                return new EntityResponse<User>($"This username is already in use: {user.UserName}");
            }

            if (await _contextProvider.Context.Users.AnyAsync(u => u.NormalizedEmail == NormalizeKey(user.Email)))
            {
                return new EntityResponse<User>($"This email address is already in use: {user.Email}");
            }

            try
            {
                var validateResult = await ValidateUserInternal(user);
                if (!validateResult.Succeeded)
                {
                    throw new Exception(validateResult.ErrorMessage);
                }

                var passwordResult = await UpdatePasswordHash(user, password);
                if (!passwordResult.Succeeded)
                {
                    throw new Exception(passwordResult.ErrorMessage);
                }

                if (roles?.Count() <= 0)
                {
                    throw new Exception("There are no roles provided.");
                }
                var entityRoles = await _contextProvider.Context.Set<Role>().ToListAsync();

                return (EntityResponse<User>)await _contextProvider.ExecuteTransactionAsync(async context =>
               {
                   context.Attach(user);
                   user.UserInformation = GenerateUserInformation(null, roles, address, campus, company, patient, personal);
                   user.SecurityStamp = NewSecurityStamp();
                   user.NormalizedEmail = NormalizeKey(user.Email);
                   user.NormalizedUserName = NormalizeKey(user.UserName);
                   await context.Users.AddAsync(user);
                   await context.SaveChangesAsync();
                   foreach (var role in roles)
                   {
                       var userRole = new UserRole()
                       {
                           UserId = user.Id,
                           RoleId = entityRoles.Single(r => r.NormalizedName == role.ToString().ToUpper()).Id
                       };
                       await context.Set<UserRole>().AddAsync(userRole);
                   }
                   await context.SaveChangesAsync();
                   return new EntityResponse<User>(user);
               });
            }
            catch (Exception e)
            {
                return new EntityResponse<User>(e.Message);
            }
        }

        private UserInformation GenerateUserInformation(UserInformation userInformation, Roles[] roles, Address address, Campus campus, Company company, Patient patient, Personal personal)
        {
            if (userInformation == null)
                userInformation = new UserInformation();

            foreach (var role in roles)
            {
                switch (role)
                {
                    case Roles.Patient:
                        userInformation.Patient = SetInformation(patient);
                        userInformation.Personal = SetInformation(personal);
                        userInformation.Address = SetInformation(address);
                        break;

                    case Roles.Intern:
                        userInformation.Campus = SetInformation(campus);
                        userInformation.Personal = SetInformation(personal);
                        userInformation.Address = SetInformation(address);
                        break;

                    case Roles.Analist:
                        userInformation.Company = SetInformation(company);
                        userInformation.Personal = SetInformation(personal);
                        userInformation.Address = SetInformation(address);
                        break;

                    case Roles.Reseacher:
                        userInformation.Company = SetInformation(company);
                        userInformation.Personal = SetInformation(personal);
                        userInformation.Address = SetInformation(address);
                        break;

                    case Roles.Surgeon:
                        userInformation.Company = SetInformation(company);
                        userInformation.Personal = SetInformation(personal);
                        userInformation.Address = SetInformation(address);
                        break;

                    case Roles.Administrator:
                        userInformation.Company = SetInformation(company);
                        break;
                }
            }

            return userInformation;
        }

        private T SetInformation<T>(T information)
        {
            if (information == null)
            {
                throw new Exception($"The information in this {typeof(T).Name} cannot be empty.");
            }

            return information;
        }

        private async Task<ServiceResponse> UpdatePasswordHash(User user, string newPassword, bool validatePassword = true)
        {
            if (validatePassword)
            {
                var result = await ValidatePasswordInternal(user, newPassword);
                if (!result.Succeeded)
                    return new ServiceResponse();
            }
            await SetPasswordHashAsync(user, newPassword != null ? _passwordHasher.HashPassword(newPassword) : null, CancellationToken);
            return new ServiceResponse();
        }
        private Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        private async Task<ServiceResponse> ValidatePasswordInternal(User user, string password)
        {
            foreach (var passwordValidator in PasswordValidators)
            {
                var identityResult = await passwordValidator.ValidateAsync(null, user, password);
                if (!identityResult.Succeeded)
                    return new ServiceResponse("Failed to validate password");
            }
            return new ServiceResponse();
        }

        private async Task<ServiceResponse> ValidateUserInternal(User user)
        {
            var errors = new List<IdentityError>();
            foreach (var userValidator in UserValidators)
            {
                var identityResult = await userValidator.ValidateAsync(null, user);
                if (!identityResult.Succeeded)
                    errors.AddRange(identityResult.Errors);
            }
            if (errors.Count <= 0)
                return new ServiceResponse();

            return new ServiceResponse("Failed to validate customerDTO");
        }

        private static string NormalizeKey(string key)
        {
            return key?.Normalize().ToUpperInvariant();
        }


        private static string NewSecurityStamp()
        {
            byte[] bytes = new byte[20];
            _rng.GetBytes(bytes);
            return Base32.ToBase32(bytes);
        }
    }
}
