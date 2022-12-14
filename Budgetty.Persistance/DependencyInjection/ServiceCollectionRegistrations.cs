using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Budgetty.Persistance.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionRegistrations
    {
        public static void RegisterDbContext(IServiceCollection services, string connectionString)
        {
            services.AddDbContextFactory<BudgettyDbContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
        }

        public static IdentityBuilder AddBudgettyDbStores(this IdentityBuilder identityBuilder)
        {
            return identityBuilder.AddEntityFrameworkStores<BudgettyDbContext>();
        }
    }
}