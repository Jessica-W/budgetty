using System.Reflection;
using Moq;

namespace Budgetty.TestHelpers;

internal static class MockHelper
{
    private static readonly MethodInfo MockGetMethodInfo = typeof(Mock).GetMethod(nameof(Mock.Get)) ?? throw new InvalidOperationException("Unable to get MethodInfo for Mock.Get");

    public static Mock? GetMockFromMockObject(object obj, Type mockType)
    {
        var mockGetTMethodInfo = MockGetMethodInfo.MakeGenericMethod(mockType) ?? throw new InvalidOperationException("Unable to make generic method");

        var mock = mockGetTMethodInfo.Invoke(null, new[] { obj }) as Mock;

        return mock;
    }
}