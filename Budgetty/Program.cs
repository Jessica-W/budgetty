using Autofac;
using Budgetty.Autofac;
using Budgetty.Persistance.Autofac;
using Budgetty.Services.Autofac;

namespace Budgetty
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<BudgettyModule>();
            containerBuilder.RegisterModule<ServicesModule>();
            containerBuilder.RegisterModule<PersistenceModule>();

            var container = containerBuilder.Build();

            var testing = container.Resolve<Testing>();

            await testing.Test();
        }
    }
}