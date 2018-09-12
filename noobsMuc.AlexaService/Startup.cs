using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace noobsMuc.AlexaService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            //https://stackoverflow.com/questions/35251078/how-to-turn-off-the-logging-done-by-the-asp-net-core-framework
            services.AddLogging(
                builder =>
                {
                    builder.AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning)
                        .AddFilter("NToastNotify", LogLevel.Warning)
                        .AddConsole();
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var loggingOptions = this.Configuration.GetSection("Log4NetCore")
                .Get<Log4NetProviderOptions>();

            loggerFactory.AddLog4Net(loggingOptions);

            app.UseMvc();
        }
    }
}
