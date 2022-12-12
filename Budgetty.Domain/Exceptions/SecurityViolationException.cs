using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Budgetty.Domain.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class SecurityViolationException : Exception
    {
        public SecurityViolationException(string message) : base(message)
        {
        }

        protected SecurityViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}