using ServerGame.Hubs;
using ServerGame.Models;

namespace ServerGame
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddDbContext<GameContext>();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors("AllowAllOrigins");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHub>("/gamehub");
            });
        }
    }
}
