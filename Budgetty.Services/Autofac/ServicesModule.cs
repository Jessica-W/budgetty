using Autofac;
using Budgetty.Services.Interfaces;

namespace Budgetty.Services.Autofac
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EventProcessor>().As<IEventProcessor>().SingleInstance();
            builder.RegisterType<FinancialsSnapshotManager>().As<IFinancialsSnapshotManager>();
            builder.RegisterType<SequenceNumberProvider>().As<ISequenceNumberProvider>();
            builder.RegisterType<SnapshotLockManager>().As<ISnapshotLockManager>();
        }
    }
}