using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Banana.Data;

namespace Banana
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
            services.AddSession(configure: s => s.IdleTimeout = TimeSpan.FromMinutes(60));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.ConfigureApplicationCookie(options => {
                options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx =>
                    {
                        ctx.Response.Redirect("/login");
                        return Task.CompletedTask;
                    },
                    OnRedirectToLogout = ctx =>
                    {
                        ctx.Response.Redirect("/logout");
                        return Task.CompletedTask;
                    },
                    // I don't want to use the default Account/AccessDenied.cshtml.
                    OnRedirectToAccessDenied = ctx =>
                    {
                        ctx.Response.Redirect("/error/403");
                        return Task.CompletedTask;
                    }
                };
            });
            services.AddDbContext<UserPageManager>(options => options.UseMySql(Configuration.GetConnectionString("BananaContextConnection")));
            services.AddTransient<UserFileManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // don't move this below UseRewriter(), or it won't work
            app.UseStatusCodePagesWithReExecute("/error/{0}");

            var rewriter = new RewriteOptions()
                .AddRewrite(@"^create/?$", "NewCourse", true)
                .AddRewrite(@"^error/(\d{3})/?$", "Error?c=$1", true)
                .AddRewrite(@"^login/?$", "Identity/Account/Login", true)
                .AddRewrite(@"^logout/?$", "Identity/Account/Logout", true)
                .AddRewrite(@"^manage/(\d+)/?$", "ManageCourse?id=$1", true)
                .AddRewrite(@"^page/(\d+)/?$", "ViewPage?id=$1", true)
                .AddRewrite(@"^page/(\d+)/edit/?$", "EditPage?id=$1", true)
                .AddRewrite(@"^page/(\d+)/source/?$", "ViewSource?id=$1", true)
                .AddRewrite(@"^page/(\d+)/verify/?$", "Verify?id=$1", true)
                .AddRewrite(@"^register/?$", "Identity/Account/Register", true)
                .AddRewrite(@"^user/?$", "UserProfile", true);
            app.UseRewriter(rewriter);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseSession();
            app.UseMvc();
        }
    }
}
