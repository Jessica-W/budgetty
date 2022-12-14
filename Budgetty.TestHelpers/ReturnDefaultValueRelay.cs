using AutoFixture.Kernel;
using Moq;

namespace Budgetty.TestHelpers;

internal class ReturnDefaultValueRelay : ISpecimenBuilder
{
    private readonly ISpecimenBuilder _nextRelay;

    public ReturnDefaultValueRelay(ISpecimenBuilder nextRelay)
    {
        _nextRelay = nextRelay;
    }

    public object? Create(object request, ISpecimenContext context)
    {
        var obj = _nextRelay.Create(request, context);

        if (request is Type type && (type.IsAbstract || type.IsInterface))
        {
            var mock = MockHelper.GetMockFromMockObject(obj, type) ?? throw new InvalidOperationException("Mock.Get failed to retrieve mock");

            mock.DefaultValue = DefaultValue.Empty;
        }

        return obj;
    }
}