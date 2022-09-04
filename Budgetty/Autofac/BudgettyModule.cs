using Autofac;
using Budgetty.DatabaseInitialisation;
using Budgetty.Persistance;
using Budgetty.Services;

namespace Budgetty.Autofac
{
    public class BudgettyModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Testing>().AsSelf();
            builder.RegisterType<Initialiser>().AsSelf();
        }
    }
}