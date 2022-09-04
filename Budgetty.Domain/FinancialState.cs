namespace Budgetty.Domain
{
    public class FinancialState
    {
        private readonly Dictionary<BudgetaryPool, int> _poolBalances = new();
        private readonly Dictionary<BankAccount, int> _bankAccountBalances = new();

        public FinancialState(List<BudgetaryPool> pools, List<BankAccount> bankAccounts,
            FinancialsSnapshot? financialsSnapshot)
        {
            foreach (var budgetaryPool in pools)
            {
                _poolBalances.Add(budgetaryPool, 0);
            }

            foreach (var bankAccount in bankAccounts)
            {
                _bankAccountBalances.Add(bankAccount, 0);
            }

            if (financialsSnapshot != null)
            {
                UnallocatedIncomeInPennies = financialsSnapshot.UnallocatedIncomeInPennies;
                foreach (var poolSnapshot in financialsSnapshot.PoolSnapshots)
                {
                    _poolBalances[poolSnapshot.Pool] = poolSnapshot.BalanceInPennies;
                }

                foreach (var bankAccountSnapShot in financialsSnapshot.BankAccountSnapShots)
                {
                    _bankAccountBalances[bankAccountSnapShot.BankAccount] = bankAccountSnapShot.BalanceInPennies;
                }
            }
        }

        public int UnallocatedIncomeInPennies { get; set; }

        public void AdjustPoolBalance(BudgetaryPool pool, int incomeAmountInPennies)
        {
            if (!_poolBalances.ContainsKey(pool))
            {
                _poolBalances.Add(pool, 0);
            }
            
            _poolBalances[pool] += incomeAmountInPennies;
            
            if (pool.BankAccount != null)
            {
                _bankAccountBalances[pool.BankAccount] += incomeAmountInPennies;
            }
        }

        public int GetPoolBalance(BudgetaryPool pool)
        {
            return _poolBalances.ContainsKey(pool)
                ? _poolBalances[pool]
                : 0;
        }

        public List<(BudgetaryPool Pool, int BalanceInPennies)> GetPoolBalancesInPennies()
        {
            return _poolBalances.Select(x => (x.Key, x.Value)).ToList();
        }

        public int GetBankAccountBalance(BankAccount bankAccount)
        {
            return _bankAccountBalances.ContainsKey(bankAccount)
                ? _bankAccountBalances[bankAccount]
                : 0;
        }
        
        public List<(BankAccount BankAccount, int BalanceInPennies)> GetBankAccountBalancesInPennies()
        {
            return _bankAccountBalances.Select(x => (x.Key, x.Value)).ToList();
        }
    }
}