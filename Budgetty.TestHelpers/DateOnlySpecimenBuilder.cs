using System.Diagnostics.CodeAnalysis;
using AutoFixture.Kernel;

namespace Budgetty.TestHelpers;

[ExcludeFromCodeCoverage]
internal class DateOnlySpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(DateOnly))
        {
            var dateTime = (DateTime)context.Resolve(typeof(DateTime));

            return DateOnly.FromDateTime(dateTime);
        }

        return new NoSpecimen();
    }
}