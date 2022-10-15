using Autofac;
using Budgetty.Mvc.Mappers;

namespace Budgetty.Mvc.DependencyInjection
{
    public class MappersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FinancialStateMapper>().As<IFinancialStateMapper>();
        }
    }
}
