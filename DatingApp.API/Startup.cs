using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DatingApp.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using DatingApp.API.Helpers;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DatingApp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        //this is for production mode
        public void ConfigureProductionServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(x => x.UseSqlServer(Configuration.GetConnectionString("SqlConnection"))
                .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.IncludeIgnoredWarning)));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
               .AddJsonOptions(opt => {
                   opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
               });
            services.AddCors(); //alows this api to be called accross domains
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings")); // this will craete instance of CloudinarySettings class defined in helpers folder and fill it with parameters from appsettings.json
            services.AddAutoMapper(); //added this one with nuget
            services.AddTransient<Seed>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IDatingRepository, DatingRepository>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                            .GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false    
                    };
                });
            //use one instance of log activity per request      
            services.AddScoped<LogUserActivity>();
        }

        //use this method if we are in development mode
        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            //services.AddDbContext<DataContext>(x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<DataContext>(x => x.UseSqlServer(Configuration.GetConnectionString("SqlConnection")));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
               .AddJsonOptions(opt => {
                   opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
               });
            services.AddCors(); //alows this api to be called accross domains
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings")); // this will craete instance of CloudinarySettings class defined in helpers folder and fill it with parameters from appsettings.json
            services.AddAutoMapper(); //added this one with nuget
            services.AddTransient<Seed>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IDatingRepository, DatingRepository>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                            .GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false    
                    };
                });
            //use one instance of log activity per request      
            services.AddScoped<LogUserActivity>();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.p
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // global exception handler in asp.ner core
                app.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                        var error = context.Features.Get<IExceptionHandlerFeature>();
                        if (error != null)
                        {
                            //AddApplicationError is extension of Response class
                            context.Response.AddApplicationError(error.Error.Message);
                            await context.Response.WriteAsync(error.Error.Message);
                        }

                    });
                });
            }

            //app.UseHttpsRedirection();
            //seeder.SeedUsers();  //uncomment if you want to populate db again
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseAuthentication();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc(routes => {
                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new {controller = "fallback", action="index"}
                );
            });
        }
    }
}
