using Budgetty.Domain.BudgetaryEvents;

namespace Budgetty.Services.Interfaces;

public interface IBudgetaryEventDescriber
{
    string DescribeEvent(BudgetaryEvent budgetaryEvent);
}