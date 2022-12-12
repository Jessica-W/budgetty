namespace Budgetty.Domain.Exceptions
{
    public class SecurityViolationException : Exception
    {
        public SecurityViolationException(string message) : base(message)
        {
        }
    }
}