using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagment.Models;
using EmployeeManagment.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagment
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddMvc();
            services.AddMvc(options=> {
                var policy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
                              })
                                  .AddXmlSerializerFormatters();
            services.AddDbContextPool<AppDataContext>(options => options.UseSqlServer(Configuration.GetConnectionString("EmployeeDbConnection")));
            services.AddScoped<IEmployeeRepository, SqlEmployeeContext>();
            //for custom authorization
            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
            services.AddSingleton<DataProtectionPurposeStrings>();
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 5;
                options.Password.RequiredUniqueChars = 1;

                //email confirmation
                options.SignIn.RequireConfirmedEmail=true;
                //account lockout
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            }).AddEntityFrameworkStores<AppDataContext>()
            //for mail confirmation
            .AddDefaultTokenProviders();
            //or for password config  
            //services.Configure<IdentityOptions>(options =>
            //    {
            //        options.Password.RequiredLength = 5;
            //        options.Password.RequiredUniqueChars = 1;

            //    }
            // );
            //change the default accessDenied path
            //services.ConfigureApplicationCookie(options => { options.AccessDeniedPath =
            //     new PathString("/Role/AccessDenied");
            //}
            //);


            services.AddAuthorization(options=> {
                options.AddPolicy("DeleteRolePolicy", 
                    policy => policy.RequireClaim("Delete Role"));

            //options.AddPolicy("EditRolePolicy",
            //  policy => policy.RequireClaim("Edit Role","true","yes"));

            //complex need three conditions (and relation)
            //options.AddPolicy("EditRolePolicy",
            //policy => policy.RequireClaim("Edit Role","true")
            //                 .RequireRole("Admin")
            //                .RequireRole("Super Admin"));

            //if we need or relation
            //options.AddPolicy("EditRolePolicy",
            //  policy => policy.RequireAssertion(context=>
            //  context.User.IsInRole("Admin")
            //  && context.User.HasClaim(claim=>claim.Type=="Edit Role" && claim.Value=="true")
            //  || context.User.IsInRole("Super Admin"))
            //  );

            //custom requirment policy
            options.AddPolicy("EditRolePolicy",
              policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirments())
       
                  );
                //options.InvokeHandlersAfterFailure = false;//to not continue the handler if on of them failure

                //role is a claim with type role
                //  options.AddPolicy("Admin Policy",
                //policy => policy.RequireClaim("Admin"));
            });
            services.AddAuthentication()
                .AddGoogle(options=> {
                options.ClientId = "546919191559-e7h6fgnunrc4ulhoi7a1pe0fgq9a3s9q.apps.googleusercontent.com";
                options.ClientSecret = "qI6Vx2E2kqSclSSBKHplnLQh";
                //,
                //options.CallbackPath();
            });

            //edit the token life span from 1 day to 5 hours
            //that will affect resetpassword token and email confirmation token
            services.Configure<DataProtectionTokenProviderOptions>(o=>
            o.TokenLifespan=TimeSpan.FromHours(5));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //  app.UseDeveloperExceptionPage();
                 app.UseStatusCodePagesWithRedirects("/errors/{0}");
              //  app.UseExceptionHandler("/error");

            }
            else
            {
                //  app.UseStatusCodePagesWithRedirects("/errors/{0}");
                app.UseExceptionHandler("/error");
            }
           // DefaultFilesOptions options = new DefaultFilesOptions();
            // options.DefaultFileNames.Clear();
            //  options.DefaultFileNames.Add("foo.html");
            //  app.UseDefaultFiles(options);
             app.UseStaticFiles();
            // app.UseFileServer();

            //  app.UseMvcWithDefaultRoute();
            app.UseAuthentication();
            app.UseMvc(route =>
            {
                route.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

           // app.UseMvc();
            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync(env.EnvironmentName);

            //    //await context.Response.WriteAsync(Configuration["Mykey"]);
            //    // await context.Response.WriteAsync(System.Diagnostics.Process.GetCurrentProcess().ProcessName);

            //});
        }
    }
}
