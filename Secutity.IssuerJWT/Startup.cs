using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Secutity.IssuerJWT.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Secutity.IssuerJWT.Configurations;
using Secutity.IssuerJWT.Models;
using System.Security.Claims;

namespace Secutity.IssuerJWT
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
               .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // configure jwt server

            services.Configure<JwtIssuer>(Configuration.GetSection(nameof(JwtIssuer)));
            services.AddTransient<IJwtIssuerOptions, JwtIssuerFactory>();


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {

                var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
                var userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                if (userManager.Users.Count() == 0)
                {
                    Task.Run(() => InitRoles(roleManager)).Wait();
                    Task.Run(() => InitUsers(userManager)).Wait();
                }
            }
        }

        private static async Task InitRoles(RoleManager<IdentityRole> roleManager)
        {
            var role = new IdentityRole("Employee");
            await roleManager.CreateAsync(role);

            role = new IdentityRole("HR-Worker");
            await roleManager.CreateAsync(role);
            await roleManager.AddClaimAsync(role, new Claim("Department", "HR"));

            role = new IdentityRole("HR-Manager");
            await roleManager.CreateAsync(role);
            await roleManager.AddClaimAsync(role, new Claim("Department", "HR"));
        }

        private static async Task InitUsers(UserManager<ApplicationUser> userManager)
        {
            var user = new ApplicationUser() { UserName = "employee", Email = "employee@xyz.com" };
            await userManager.CreateAsync(user, "password");
            await userManager.AddToRoleAsync(user, "Employee");

            user = new ApplicationUser() { UserName = "hrworker", Email = "hrworker@xyz.com" };
            await userManager.CreateAsync(user, "password");
            await userManager.AddToRoleAsync(user, "Employee");
            await userManager.AddToRoleAsync(user, "HR-Worker");

            user = new ApplicationUser() { UserName = "hrmanager", Email = "hrmanager@xyz.com" };
            await userManager.CreateAsync(user, "password");
            await userManager.AddToRoleAsync(user, "Employee");
            await userManager.AddToRoleAsync(user, "HR-Worker");
            await userManager.AddToRoleAsync(user, "HR-Manager");

            await userManager.AddClaimAsync(user, new Claim("CeoApproval", "true"));
        }

    }
}
