using Autofac;

namespace Budgetty.Persistance.Autofac
{
    public class PersistenceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BudgettyDbContext>().AsSelf().InstancePerLifetimeScope();
        }
    }
}