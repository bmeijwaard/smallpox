using Microsoft.AspNetCore.Identity;
using Smallpox.Entities;
using Smallpox.Entities.Information;
using Smallpox.Entities.Types;
using Smallpox.Persistence;
using Smallpox.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Smallpox.Config
{
    public class RoleSeeder
    {
        private readonly RoleManager<Role> _roleManager;

        public RoleSeeder(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task Seed()
        {
            foreach (var role in Enum.GetNames(typeof(Roles)))
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new Role(role));
                }
            }
        }
    }

    public class UserSeeder
    {
        private readonly IDbContextProvider _context;
        private readonly IUserService _userService;

        public UserSeeder(IDbContextProvider context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task Seed()
        {
            try
            {
                if (!_context.Context.Users.Any())
                {
                    var admin = new User
                    {
                        Email = "info@bobdebouwer.nl",
                        UserName = "Bobdebouwer",
                        EmailConfirmed = true
                    };

                    //dev settings
                    var result = await _userService.CreateAdminAsync(admin, new Company { CompanyProp = "Bob de bouwer Inc." }, "Test1234!");

                    if (!result.Succeeded)
                    {
                        throw new Exception(result.ErrorMessage);
                    }
                }

            }
            catch
            {
                //ignore
            }
        }
    }
}
