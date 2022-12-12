using Autofac;
using Budgetty.Services.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Budgetty.Services.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EventProcessor>().As<IEventProcessor>().SingleInstance();
            builder.RegisterType<FinancialsSnapshotManager>().As<IFinancialsSnapshotManager>();
            builder.RegisterType<FinancialStateService>().As<IFinancialStateService>();
            builder.RegisterType<DateTimeProvider>().As<IDateTimeProvider>().SingleInstance();
            builder.RegisterType<BudgetaryEventFactory>().As<IBudgetaryEventFactory>();
        }
    }
}