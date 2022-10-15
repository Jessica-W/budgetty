using Autofac;
using Budgetty.Persistance.Repositories;

namespace Budgetty.Persistance.DependencyInjection
{
    public class PersistenceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BudgetaryRepository>().As<IBudgetaryRepository>();

            builder.RegisterType<SequenceNumberProvider>().As<ISequenceNumberProvider>();
            builder.RegisterType<SnapshotLockManager>().As<ISnapshotLockManager>();
        }
    }
}