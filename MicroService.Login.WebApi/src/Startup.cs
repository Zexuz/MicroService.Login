using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autofac;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;
using MicroService.Common.Core.Loggin;
using MicroService.Login.Models;
using MicroService.Login.Repo.Sql.Services;
using MicroService.Login.Security.Helpers;
using MicroService.Login.WebApi.Filters;
using MicroService.Login.WebApi.Middlewares;
using Swashbuckle.AspNetCore.Swagger;

namespace MicroService.Login.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (Convert.ToBoolean(Configuration["Migration"]))
            {
                var tableHelper = new TableHelper(new SqlConnectionFactory(Configuration["MSSQL:ConnectionString:Master"]));
                var migrationService = new MigrationService(tableHelper);
                Console.WriteLine("Migration database started");
                migrationService.CleanDatabase("Coins").Wait();
                Console.WriteLine("Migration database finshied");
            }

            Log.ConfigureElasticSearachLog(Configuration["ElasticSearch:Url"]);

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.EnvironmentName.ToLower().Contains("dev"))
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger().UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "HTTP API V1"); });
                app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials().Build());
            }
            else
            {
                app.UseHsts(options => options.MaxAge(days: 365).IncludeSubdomains());
                app.UseXXssProtection(options => options.EnabledWithBlockMode());
                app.UseXContentTypeOptions();
                app.UseXfo(options => options.Deny());
            }

            app.UseAuthentication();
            app.UseMiddleware<SerilogMiddleware>();
            app.UseMvc();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacModule(Configuration));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AddSwagger(services);
            AddAuthentication(services);

            services.AddMvc(options =>
            {
                options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
                options.Filters.Add(typeof(ValidateModelStateFilter));
                options.Filters.Add(typeof(SameIpOriginRequirementFilter));
            });
        }


        private void AddAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(options => { options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Hosting:Domain"],
                        ValidAudience = Configuration["Hosting:Domain"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"])),
                        LifetimeValidator = (before, expires, token, parameters) =>
                        {
                            var now = DateTimeOffset.UtcNow;
                            var res = expires > now;
                            return res;
                        },
                        RoleClaimType = "ro",
                        NameClaimType = "id",
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", p => p.RequireAuthenticatedUser()
                    .RequireRole(RoleParser.ToInt(Role.Admin).ToString()));
            });
        }

        private static void AddSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.DescribeStringEnumsInCamelCase();
                options.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme."           +
                                  "\n(DO NOT STORE THIS IN A COOKIE, ONLY IN LOCALSTORAGE!!!!)" +
                                  "\n Example: \"Authorization: Bearer {token}\", "             +
                                  "\n Get this token from POST: account/login (It's in the response header)",
                    Type = "apiKey",
                    In = "header",
                    Name = "Authorization"
                });

                options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }}
                });

                options.SwaggerDoc("v1", new Info
                {
                    Title = "HTTP API",
                    Version = "v1",
                    Description = "The Service HTTP API",
                });

                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "WebApi.xml");
                options.IncludeXmlComments(xmlPath);
            });
        }
    }
}