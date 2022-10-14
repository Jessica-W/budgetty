using Autofac;
using Autofac.Extensions.DependencyInjection;
using Budgetty.Mvc.Identity;
using Budgetty.Persistance;
using Budgetty.Persistance.Autofac;
using Budgetty.Services.Autofac;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Budgetty.Mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<BudgettyDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<BudgettyDbContext>()
                .AddUserManager<ApplicationUserManager>();

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

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}