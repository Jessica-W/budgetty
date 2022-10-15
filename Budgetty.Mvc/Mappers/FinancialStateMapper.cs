using Budgetty.Domain;
using Budgetty.Mvc.Models.Summary;

namespace Budgetty.Mvc.Mappers
{
    public class FinancialStateMapper : IFinancialStateMapper
    {
        public SummaryViewModel MapToSummaryViewModel(FinancialState financialState)
        {
            var poolBalances = financialState.GetPoolBalancesInPennies();

            return new SummaryViewModel
            {
                UnallocatedIncome = (decimal)financialState.UnallocatedIncomeInPennies / 100,
                BankAccounts = financialState.GetBankAccountBalancesInPennies()
                    .Select(x => new BankAccountViewModel
                    {
                        AccountBalance = (decimal)x.BalanceInPennies / 100,
                        AccountName = x.BankAccount.Name,
                        IncomePoolBalances = poolBalances
                            .Where(p => p.Pool.BankAccount != null && p.Pool.BankAccount.Id == x.BankAccount.Id && p.Pool.Type == PoolType.Income)
                            .Select(p => (p.Pool.Name, (decimal)p.BalanceInPennies / 100))
                            .ToList(),
                    })
                    .ToList(),
                DebtPoolBalances = poolBalances
                    .Where(x => x.Pool.Type == PoolType.Debt)
                    .Select(p => (p.Pool.Name, -(decimal)p.BalanceInPennies / 100))
                    .ToList(),
            };
        }
    }
}
