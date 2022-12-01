using Autofac;
using Autofac.Extensions.DependencyInjection;
using Budgetty.Mvc.DependencyInjection;
using Budgetty.Mvc.Identity;
using Budgetty.Persistance.DependencyInjection;
using Budgetty.Services.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace Budgetty.Mvc
{
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

            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "512944185798-32mbpb62mbsut9j3pc2kelf1rt4eho41.apps.googleusercontent.com";
                    options.ClientSecret = "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";
                });

            builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.RegisterModule<ServicesModule>();
                builder.RegisterModule<PersistenceModule>();
                builder.RegisterModule<MappersModule>();
            });

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

            //app.UseHttpsRedirection();
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
    }
}