using Autofac;
using Autofac.Extensions.DependencyInjection;
using Budgetty.Mvc.DependencyInjection;
using Budgetty.Mvc.Identity;
using Budgetty.Mvc.Middleware;
using Budgetty.Persistance.DependencyInjection;
using Budgetty.Services.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Budgetty.Mvc
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            ServiceCollectionRegistrations.RegisterDbContext(builder.Services, connectionString);
            
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddBudgettyDbStores()
                .AddUserManager<ApplicationUserManager>();

            builder.Services.AddDateOnlyTimeOnlyStringConverters();

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<LogCsrfFailureFilter>();
            });

            AddGoogleOAuth(builder.Configuration, builder.Services);

            AddApplicationInsights(builder.Configuration, builder.Services);

            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterModule<ServicesModule>();
                containerBuilder.RegisterModule<PersistenceModule>();
                containerBuilder.RegisterModule<MappersModule>();
            });

            builder.Services.AddTransient<ILogger, MyLogger>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMiddleware<LoggingMiddleware>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Summary}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }

        private static void AddGoogleOAuth(ConfigurationManager configuration, IServiceCollection services)
        {
            var googleOAuthConfig = GetConfig<GoogleOAuthConfig>(configuration, "GoogleOAuth");

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = googleOAuthConfig.ClientId;
                    options.ClientSecret = googleOAuthConfig.ClientSecret;
                });
        }

        private static void AddApplicationInsights(IConfiguration configuration, IServiceCollection services)
        {
            var appInsightsConfig = GetConfig<AppInsightsConfig>(configuration, "AppInsights");

            var applicationInsightsOptions = new ApplicationInsightsServiceOptions
            {
                ConnectionString = appInsightsConfig.ConnectionString,
                EnableAdaptiveSampling = false,
                EnableQuickPulseMetricStream = appInsightsConfig.EnableQuickPulseMetricStream,
                EnableEventCounterCollectionModule = appInsightsConfig.EnableEventCounterCollectionModule,
                EnablePerformanceCounterCollectionModule = appInsightsConfig.EnablePerformanceCounterCollectionModule,
                EnableHeartbeat = appInsightsConfig.EnableHeartbeat,
            };

            services.AddApplicationInsightsTelemetry(applicationInsightsOptions);
        }

        private static T GetConfig<T>(IConfiguration configuration, string key)
        {
            return configuration.GetSection(key).Get<T>();
        }
    }

    public class MyLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null!;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }
    }
}