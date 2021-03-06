using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SevenStuds.Hubs; // new

namespace SevenStuds
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
            System.Diagnostics.Debug.WriteLine("Getting env var SevenStudsOrigin");
            string originUrl = Environment.GetEnvironmentVariable("SevenStudsOrigin", EnvironmentVariableTarget.Process);
            if ( originUrl == null ) {
                originUrl = "http://localhost:3000"; // or e.g. "https://7studsserver.azurewebsites.net/"                
                System.Diagnostics.Debug.WriteLine("Env var SevenStudsOrigin not found, setting to default value, "+originUrl);

            }
            else {
                System.Diagnostics.Debug.WriteLine("SevenStudsOrigin="+originUrl);
            }

            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithOrigins(originUrl)
                    .AllowCredentials();
            }));
            services.AddRazorPages();
            services.AddSignalR(o =>
            {
                //o.EnableDetailedErrors = true;
                o.MaximumReceiveMessageSize = null; // See https://stackoverflow.com/questions/59248464/how-to-change-signalr-maximum-message-size
            }); // new
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<ChatHub>("/chathub");
            });

        }
    }
}
