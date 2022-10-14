namespace Budgetty
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var name = "test";
            Test($"This is a {name}");

            await Task.Delay(1);
        }

        private static void Test(FormattableString fmtString)
        {
            var fmt = fmtString.Format;
        }
    }
}