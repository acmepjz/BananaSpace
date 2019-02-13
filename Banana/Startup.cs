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

            services.AddDbContext<UserPageManager>(options => options.UseSqlServer(Configuration.GetConnectionString("BananaContextConnection")));
            services.AddTransient<UserFileManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var rewriter = new RewriteOptions()
                .AddRewrite(@"^create/?$", "NewCourse", true)
                .AddRewrite(@"^login/?$", "Identity/Account/Login", true)
                .AddRewrite(@"^logout/?$", "Identity/Account/Logout", true)
                .AddRewrite(@"^manage/([0-9]+)/?$", "ManageCourse?id=$1", true)
                .AddRewrite(@"^page/([0-9]+)/?$", "ViewPage?id=$1", true)
                .AddRewrite(@"^page/([0-9]+)/edit/?$", "EditPage?id=$1", true)
                .AddRewrite(@"^page/([0-9]+)/source/?$", "ViewSource?id=$1", true)
                .AddRewrite(@"^page/([0-9]+)/verify/?$", "Verify?id=$1", true)
                .AddRewrite(@"^register/?$", "Identity/Account/Register", true)
                .AddRewrite(@"^user/?$", "UserProfile", true);
            app.UseRewriter(rewriter);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseStatusCodePagesWithReExecute("/Error");
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseSession();
            app.UseMvc();
        }
    }
}
