using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Security.ApiApplication.Configurations;
using Security.ApiApplication.Extentions;

namespace Security.ApiApplication
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
            services.Configure<JwtValidationOption>(Configuration.GetSection(nameof(JwtValidationOption)));
            services.AddSingleton<IJwtValidationOption, JwtValidationOptionFactory>();
            var tokenValidationSettings = services.BuildServiceProvider().GetService<IJwtValidationOption>();
            services.AddSwaggerDocumentation();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(jwt =>
                    {
                        jwt.TokenValidationParameters = tokenValidationSettings.CreateTokenValidationParameters();
                        jwt.SaveToken = true;

                    });

            var authorizePolicy = new AuthorizationPolicyBuilder()
                          .RequireAuthenticatedUser()
                          .Build();

            // Add Mvc with options
            services.AddMvc(config => { config.Filters.Add(new AuthorizeFilter(authorizePolicy)); });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                 .AddJsonOptions(options =>
                 {
                     options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                     options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                 }); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwaggerDocumentation();
            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
