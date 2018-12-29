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
using Microsoft.AspNetCore.Identity;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

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
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            //services.AddDbContext<DataContext>(x => x.UseSqlServer(Configuration.GetConnectionString("SqlConnection"))
            //    .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.IncludeIgnoredWarning)));
            
            //Identity configuration goes here
            //AddIdentity includes cookies and automatic redurect to login page and it uses Razor views
            //this is good choice for MVC 
            //this app is based on angular not razor views. Therefore - AddIdentityCore.
            //AddIdentity core is a shell for Identity , so you'll have to add peices you need and can avoid
            //using cookies-based authentication
            IdentityBuilder builder = services.AddIdentityCore<User>(opt => {
                //al theses setting allow for weal password for testing 
                opt.Password.RequireDigit = false;
                opt.Password.RequiredLength = 4;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
            });
            
            //settings below allows to bringback Users along with their roles
            
            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
            //tell Identity framework that we will use Entity Framework as our store
            //and Identity framework will add user classes to a database
            //note: if we used addIdentity and not AddIdentity core, these would have been there already
            builder.AddEntityFrameworkStores<DataContext>();
            builder.AddRoleValidator<RoleValidator<Role>>();
            builder.AddRoleManager<RoleManager<Role>>();
            builder.AddSignInManager<SignInManager<User>>();
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

            //add policy based Authorization
            services.AddAuthorization(options => {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin","Moderator"));
                options.AddPolicy("VipOnle", policy => policy.RequireRole("VIP"));
            });

            //added options to AddMvc() so that every request is automatically authenticated
            //this allows to remove Authorize Attribute from Controller     
            services.AddMvc(options => 
                {
                    var policy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                }
            )
               .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
               .AddJsonOptions(opt => {
                   opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
               });
            services.AddCors(); //alows this api to be called accross domains
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings")); // this will craete instance of CloudinarySettings class defined in helpers folder and fill it with parameters from appsettings.json
           
            Mapper.Reset(); //SHOULD BE USED ONECE IN DEV MODE BECAUSE CONFIGURESERVICIES IS CALLED
            //TWICE WHEN drop databse command is issued. this is needed to load identity frmwrk tables 
            services.AddAutoMapper(); //added this one with nuget
            services.AddTransient<Seed>();
            //services.AddScoped<IAuthRepository, AuthRepository>();//no longer needed simce authrepository is replaced
            //with Identity framework
            services.AddScoped<IDatingRepository, DatingRepository>();
            
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
            seeder.SeedUsers();  //uncomment if you want to populate db again
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
