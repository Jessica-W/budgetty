using Budgetty.Domain;
using Budgetty.Mvc.Models.Summary;

namespace Budgetty.Mvc.Mappers;

public interface IFinancialStateMapper
{
    SummaryViewModel MapToSummaryViewModel(FinancialState financialState);
}