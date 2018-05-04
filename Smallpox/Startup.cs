using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smallpox.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Smallpox.Persistence;
using System;
using Smallpox.Config.Settings;
using Microsoft.EntityFrameworkCore.Design;
using Smallpox.Config;
using Smallpox.Services.Identity;
using Microsoft.AspNetCore.Http;
using Smallpox.Services;

namespace Smallpox
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }
        private IHostingEnvironment Env { get; }
        public IConfiguration Configuration { get; }
        public ServiceProvider ServiceProvider { get; private set; }
        public RoleSeeder RoleSeeder { get; private set; }
        public UserSeeder UserSeeder { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ConnectionStrings>(opt => Configuration.GetSection("ConnectionStrings").Bind(opt));

            services.AddDbContext<SqlContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, Role>(options =>
            {
                options.Lockout = new LockoutOptions
                {
                    AllowedForNewUsers = true,
                    DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30),
                    MaxFailedAccessAttempts = 5
                };
            })
            .AddEntityFrameworkStores<SqlContext>()
            .AddDefaultTokenProviders()
            .AddUserStore<UserStore<User, Role, SqlContext, Guid>>()
            .AddRoleStore<RoleStore<Role, SqlContext, Guid>>()
            .AddUserManager<UserManager>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = true;

            });

            // specific injection of DbContext to ensure thread safety.
            services.AddScoped<IDbContext>(provider => provider.GetService<SqlContext>());
            services.AddScoped<IDbContextProvider, DbContextProvider>();

            // alterative options to use delegates, but that needs more refinement.
            services.AddScoped(provider => new Func<SqlContext>(provider.GetService<SqlContext>));

            // providers
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<ISignInManager, SignInManager>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // services
            services.AddScoped<IUserService, UserService>();

            services.AddMvc();

            services.AddTransient<RoleSeeder>();
            services.AddTransient<UserSeeder>();

            ServiceProvider = services.BuildServiceProvider();
            RoleSeeder = ServiceProvider.GetService<RoleSeeder>();
            UserSeeder = ServiceProvider.GetService<UserSeeder>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                RoleSeeder.Seed().Wait();
                UserSeeder.Seed().Wait();

                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public class SqlContextFactory : IDesignTimeDbContextFactory<SqlContext>
    {
        public SqlContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var builder = new DbContextOptionsBuilder<SqlContext>();
            builder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            return new SqlContext(builder.Options);
        }
    }
}
