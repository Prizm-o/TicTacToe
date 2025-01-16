using System;
using TicTacToe.Data;
using TicTacToe.Hubs;

namespace Server
{
    public class Start
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddDbContext<AppDbContext>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHub>("/gamehub");
            });
        }

    }
}
