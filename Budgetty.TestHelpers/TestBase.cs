using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using Moq;
using NUnit.Framework;

namespace Budgetty.TestHelpers;

[ExcludeFromCodeCoverage]
public abstract class TestBase<TClassUnderTest> : IMockCreationSynchronizer where TClassUnderTest : class
{
    private const string DefaultMockName = "default";

    // ReSharper disable once StaticMemberInGenericType
    private static readonly IFixture TestDataFixture;

    private readonly MethodInfo _fixtureRegistrarInjectMethodInfo =
        typeof(FixtureRegistrar).GetMethod(nameof(FixtureRegistrar.Inject)) ??
        throw new InvalidOperationException("Unable to get MethodInfo for FixtureRegistrar.Inject");

    private TClassUnderTest? _classUnderTest;

    private IFixture? _fixture;
    private Dictionary<Type, Dictionary<string, Mock>> _injectedMocks = new();

    static TestBase()
    {
        TestDataFixture = CreateNewDataFixture();
    }

    protected TClassUnderTest ClassUnderTest
    {
        get
        {
            EnsureSetupWasCalled();

            return _classUnderTest ??= CreateDefaultClassUnderTest();
        }
    }

    public void SynchronizeMock(Type type, Mock mock)
    {
        if (_injectedMocks.ContainsKey(type))
        {
            throw new InvalidOperationException(
                $"Duplicate mock for {type.Name} detected. Fixture can only have one mock per type.");
        }

        _injectedMocks.Add(type, new Dictionary<string, Mock> { { DefaultMockName, mock } });
        InjectMock(type, mock);
    }

    [SetUp]
    protected void BaseSetUp()
    {
        _fixture = CreateNewFixture();
        _injectedMocks = new Dictionary<Type, Dictionary<string, Mock>>();
        _classUnderTest = null;

        SetUp();
    }

    protected abstract void SetUp();

    protected Mock<TMock> GetMock<TMock>(string name = DefaultMockName) where TMock : class
    {
        EnsureSetupWasCalled();

        var namedMocks = _injectedMocks.GetValueOrDefault(typeof(TMock));

        if (namedMocks == null)
        {
            namedMocks = new Dictionary<string, Mock>();
            _injectedMocks.Add(typeof(TMock), namedMocks);
        }

        var mock = namedMocks.GetValueOrDefault(name);

        if (mock == null)
        {
            mock = _fixture.Create<Mock<TMock>>();

            namedMocks.Add(name, mock);

            if (name == DefaultMockName)
                // Needed so the fixture can satisfy the constructor for TClassUnderTest
                _fixture.Inject((TMock)mock.Object);
        }

        return (Mock<TMock>)mock;
    }

    protected virtual TClassUnderTest CreateDefaultClassUnderTest()
    {
        return _fixture
            ?.Build<TClassUnderTest>()
            .OmitAutoProperties()
            .Create() ?? throw new InvalidOperationException("Unable to create class under test");
    }

    protected static ICustomizationComposer<TObject> BuildObject<TObject>()
    {
        return TestDataFixture.Build<TObject>();
    }

    protected static TObject CreateObject<TObject>()
    {
        return TestDataFixture.Create<TObject>();
    }

    protected static IEnumerable<TObject> CreateManyObjects<TObject>(int? count = null)
    {
        return count != null
            ? TestDataFixture.Build<TObject>().CreateMany(count.Value)
            : TestDataFixture.Build<TObject>().CreateMany();
    }

    private void InjectMock(Type type, Mock mock)
    {
        EnsureSetupWasCalled();

        var injectMethodInfo = _fixtureRegistrarInjectMethodInfo.MakeGenericMethod(type);
        injectMethodInfo.Invoke(null, new[] { _fixture, mock.Object });
    }

    private static IFixture CreateNewDataFixture()
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new DateOnlySpecimenBuilder());
        fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(x => fixture.Behaviors.Remove(x));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior(3));

        return fixture;
    }

    private IFixture CreateNewFixture()
    {
        var autoMoqCustomization = new AutoMoqCustomization();
        var oldRelay = autoMoqCustomization.Relay;
        autoMoqCustomization.Relay = new ReturnDefaultValueRelay(new MockSynchronizerRelay(this, oldRelay));
        autoMoqCustomization.ConfigureMembers = false;

        var fixture = new Fixture();
        fixture.Customizations.Add(new SingularIEnumerableSpecimenBuilder());
        fixture.Customize(autoMoqCustomization);

        return fixture;
    }

    private void EnsureSetupWasCalled()
    {
        if (_fixture == null) throw new InvalidOperationException("Setup() must be called but wasn't");
    }
}

internal class SingularIEnumerableSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is ParameterInfo pi && typeof(IEnumerable).IsAssignableFrom(pi.ParameterType))
        {
            var pt = pi.ParameterType;
            var et = pt.GetGenericArguments().FirstOrDefault();
            
            if (et != null)
            {
                var obj = context.Resolve(et);
                var lstTType = typeof(List<>).MakeGenericType(et);
                var lstT = Activator.CreateInstance(lstTType, BindingFlags.Public);
                var add = lstTType.GetMethod(nameof(List<int>.Add)); // int arbitrarily used just because you need to specify a type for nameof

                if (add != null && lstT != null)
                {
                    add.Invoke(lstT, BindingFlags.Public, null, new[] { obj }, null);
                    return lstT;
                }
            }
        }

        return new NoSpecimen();
    }
}