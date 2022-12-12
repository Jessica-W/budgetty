using System.Reflection;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Dsl;
using Moq;
using NUnit.Framework;

namespace Budgetty.TestHelpers;

public class TestBase<TClassUnderTest> : IMockCreationSynchronizer where TClassUnderTest : class
{
    private const string DefaultMockName = "default";

    // ReSharper disable once StaticMemberInGenericType
    private static readonly IFixture TestDataFixture;

    private readonly MethodInfo _fixtureRegistrarInjectMethodInfo =
        typeof(FixtureRegistrar).GetMethod(nameof(FixtureRegistrar.Inject)) ?? throw new InvalidOperationException("Unable to get MethodInfo for FixtureRegistrar.Inject");

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
            throw new InvalidOperationException($"Duplicate mock for {type.Name} detected. Fixture can only have one mock per type.");
        }

        _injectedMocks.Add(type, new Dictionary<string, Mock> { { DefaultMockName, mock } });
        InjectMock(type, mock);
    }

    private void InjectMock(Type type, Mock mock)
    {
        EnsureSetupWasCalled();

        var injectMethodInfo = _fixtureRegistrarInjectMethodInfo.MakeGenericMethod(type);
        injectMethodInfo.Invoke(null, new[] { _fixture, mock.Object });
    }

    [SetUp]
    protected void BaseSetUp()
    {
        _fixture = CreateNewFixture();
        _injectedMocks = new Dictionary<Type, Dictionary<string, Mock>>();
        _classUnderTest = null;

        SetUp();
    }

    protected virtual void SetUp()
    {
    }

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
        return _fixture.Create<TClassUnderTest>();
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

    private static IFixture CreateNewDataFixture()
    {
        return new Fixture();
    }

    private IFixture CreateNewFixture()
    {
        var autoMoqCustomization = new AutoMoqCustomization();
        var oldRelay = autoMoqCustomization.Relay;
        autoMoqCustomization.Relay = new ReturnDefaultValueRelay(new MockSynchronizerRelay(this, oldRelay));

        var fixture = new Fixture();
        fixture.Customize(autoMoqCustomization);

        return fixture;
    }

    private void EnsureSetupWasCalled()
    {
        if (_fixture == null) throw new InvalidOperationException("Setup() must be called but wasn't");
    }
}