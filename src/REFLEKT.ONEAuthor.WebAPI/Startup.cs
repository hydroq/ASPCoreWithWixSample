using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using REFLEKT.ONEAuthor.Application;
using REFLEKT.ONEAuthor.Application.Authorization;
using REFLEKT.ONEAuthor.Application.Helpers;
using REFLEKT.ONEAuthor.Application.Licensing;
using REFLEKT.ONEAuthor.Application.Utilities;
using REFLEKT.ONEAuthor.WebAPI.Authentication.Schemes.Basic;
using REFLEKT.ONEAuthor.Application.Scenarios;
using REFLEKT.ONEAuthor.Application.Settings;
using REFLEKT.ONEAuthor.Application.Topics;

namespace REFLEKT.ONEAuthor.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;

            ApplicationLogging.LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            services
                .AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });

            services
                .AddAuthentication(options => { options.DefaultScheme = "Basic"; })
                .AddBasic();

            // DI registration
            services.AddScoped<LocaleConverter, LocaleConverter>();
            services.AddScoped<IJsonSerializerUtility, JsonSerializerUtility>();
            services.AddScoped<ISettingsManager, SettingsManager>();
            services.AddScoped<IScenarioService, ScenarioService>();
            services.AddScoped<ITopicService, TopicService>();
            services.AddScoped<IAuthenticationManager, AuthenticationManager>();
            services.AddScoped<Scenarios, Scenarios>();
            services.AddScoped<LicenseManager, LicenseManager>(provider => new LicenseManager(provider.GetService<ILogger<LicenseManager>>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}