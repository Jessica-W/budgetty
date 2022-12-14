using System.Diagnostics.CodeAnalysis;

namespace Budgetty.Mvc.Models.Pools
{
    public class PoolsViewModel
    {
        public List<PoolViewModel> Pools { get; set; } = new();
    }

    public class PoolViewModel
    {
        public int Id { get; set; }

        [ExcludeFromCodeCoverage]
        public string Name { get; set; } = string.Empty;

        public bool Deletable { get; set; }
        public string BankAccountName { get; set; } = string.Empty;
    }
}