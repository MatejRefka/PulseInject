using System.Text;

namespace PulseInject.Tests;

public class DependencyResolverTests
{
    #region Arrangement Classes
    public sealed class ServiceA
    {
        public ServiceA(ServiceB service)
        {
        }
    }

    public sealed class ServiceB
    {
        public ServiceB(ServiceA service)
        {
        }
    }

    public sealed class ServiceC
    {
        public List<int> ListA { get; }
        public List<int> ListB { get; }

        public ServiceC(List<int> listA, List<int> listB)
        {
            ListA = listA;
            ListB = listB;
        }
    }

    public sealed class ServiceD
    {
        public int Value { get; }

        public ServiceD(int value = 12)
        {
            Value = value;
        }
    }

    public sealed class ServiceE
    {
        public int Value { get; }

        public ServiceE(int value)
        {
            Value = value;
        }
    }

    public sealed class ServiceF
    {
        public StringBuilder Builder { get; }

        public ServiceF(StringBuilder builder)
        {
            Builder = builder;
        }
    }

    public sealed class ServiceG
    {
        public IList<int> Values { get; }

        public ServiceG(IList<int> values)
        {
            Values = values;
        }
    }

    public sealed class ServiceH
    {
        public IList<int> ListA { get; }
        public IList<int> ListB { get; }

        public ServiceH(IList<int> listA, IList<int> listB)
        {
            ListA = listA;
            ListB = listB;
        }
    }

    #endregion Arrangement Classes

    //TEST 1
    [Fact]
    public void GetType_ThrowForTransientOrScopedDependenciesWithinSingletonService()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddSingleton<StreamReader>();
        container.AddScoped<Stream, MemoryStream>();

        var resolver = new DependencyResolver(container);

        //act & assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            var service = resolver.GetService<StreamReader>();
        });
    }

    //TEST 2
    [Fact]
    public void GetType_DetectCircularDependency()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddSingleton<ServiceA>();
        container.AddSingleton<ServiceB>();

        var resolver = new DependencyResolver(container);

        //act & assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            var service = resolver.GetService<ServiceA>();
        });
    }

    //TEST 3
    [Fact]
    public void GetType_ReturnCachedInstance()
    {
        //arrange
        DependencyContainer container = new();
        container.AddScoped<List<int>>();
        container.AddScoped<ServiceC>();

        DependencyResolver resolver = new(container);

        //act
        ServiceC service = Assert.IsType<ServiceC>(resolver.GetService<ServiceC>());

        //assert
        Assert.Same(service.ListA, service.ListB);
    }

    //TEST 4
    [Fact]
    public void GetType_GetSingletonInstance()
    {
        //arrange
        DependencyContainer container = new();
        container.AddSingleton<StringBuilder>();

        DependencyResolver resolver = new(container);

        //act
        StringBuilder firstService = Assert.IsType<StringBuilder>(resolver.GetService<StringBuilder>());
        StringBuilder secondService = Assert.IsType<StringBuilder>(resolver.GetService<StringBuilder>());

        //assert
        Assert.Same(firstService, secondService);
    }

    //TEST 5
    [Fact]
    public void GetType_ResolveRegisteredValue()
    {
        //arrange
        DependencyContainer container = new();
        container.AddSingleton(true);
        container.AddSingleton("first");
        container.AddSingleton("second");

        DependencyResolver resolver = new(container);

        //act
        bool boolService = resolver.GetService<bool>();
        string? stringService = resolver.GetService<string>();

        //assert
        Assert.True(boolService);
        Assert.Equal("second", stringService);
    }

    //TEST 6
    [Fact]
    public void GetType_ResolveDeclaredDefault()
    {
        //arrange
        DependencyContainer container = new();
        container.AddSingleton<ServiceD>();

        DependencyResolver resolver = new(container);

        //act
        ServiceD service = Assert.IsType<ServiceD>(resolver.GetService<ServiceD>());

        //assert
        Assert.Equal(12, service.Value);
    }

    //TEST 7
    [Fact]
    public void GetType_ThrowForUndeclaredDefault()
    {
        //arrange
        DependencyContainer container = new();

        DependencyResolver resolver = new(container);

        //act & assert
        Assert.Throws<InvalidOperationException>(() => resolver.GetService<int>());
    }

    //TEST 8
    [Fact]
    public void GetType_ThrowForUnregisteredAbstractType()
    {
        //arrange
        DependencyContainer container = new();

        DependencyResolver resolver = new(container);

        //act & assert
        Assert.Throws<InvalidOperationException>(() => resolver.GetService<IList<int>>());
    }

    //TEST 9
    [Fact]
    public void GetType_ThrowForUnregisteredTopLevelConcreteType()
    {
        //arrange
        DependencyContainer container = new();
        container.AddSingleton<ServiceF>();

        DependencyResolver resolver = new(container);

        //act & assert
        Assert.Throws<InvalidOperationException>(() => resolver.GetService<StringBuilder>());
    }

    //TEST 10
    [Fact]
    public void GetType_ThrowForUnregisteredAbstractConstructorParameter()
    {
        //arrange
        DependencyContainer container = new();
        container.AddSingleton<ServiceG>();

        DependencyResolver resolver = new(container);

        //act & assert
        Assert.Throws<NotImplementedException>(() => resolver.GetService<ServiceG>());
    }

    //TEST 11
    [Fact]
    public void GetType_ThrowForUndeclaredConstructorDefault()
    {
        //arrange
        DependencyContainer container = new();
        container.AddSingleton<ServiceE>();

        DependencyResolver resolver = new(container);

        //act & assert
        Assert.Throws<NotImplementedException>(() => resolver.GetService<ServiceE>());
    }

    //TEST 12
    [Fact]
    public void GetType_ResolveUnregisteredConcreteTypeAsDependency()
    {
        //arrange
        DependencyContainer container = new();
        container.AddSingleton<ServiceF>();

        DependencyResolver resolver = new(container);

        //act
        ServiceF service = Assert.IsType<ServiceF>(resolver.GetService<ServiceF>());

        //assert
        Assert.IsType<StringBuilder>(service.Builder);
    }

    //TEST 13
    [Fact]
    public void GetType_GetRegisteredReferenceTypeInstance()
    {
        //arrange
        DependencyContainer container = new();
        StringBuilder builder = new("Hello");
        container.AddSingleton(builder);

        DependencyResolver resolver = new(container);

        //act
        StringBuilder service = Assert.IsType<StringBuilder>(resolver.GetService<StringBuilder>());

        //assert
        Assert.Same(builder, service);
    }

    //TEST 14
    [Fact]
    public void GetType_RegisteredValueTakesPrecedenceOverDeclaredDefault()
    {
        //arrange
        DependencyContainer container = new();
        container.AddSingleton<ServiceD>();
        container.AddSingleton(42);

        DependencyResolver resolver = new(container);

        //act
        ServiceD service = Assert.IsType<ServiceD>(resolver.GetService<ServiceD>());

        //assert
        Assert.Equal(42, service.Value);
    }

    //TEST 15
    [Fact]
    public void GetType_ResolveAbstractService()
    {
        //arrange
        DependencyContainer container = new();
        container.AddSingleton<ICollection<int>, List<int>>();
        container.AddSingleton<ICollection<int>, HashSet<int>>();

        DependencyResolver resolver = new(container);

        //act
        ICollection<int>? service = resolver.GetService<ICollection<int>>();

        //assert
        Assert.IsType<HashSet<int>>(service);
    }

    //TEST 16
    [Fact]
    public void GetType_ResolveSharedAbstractType()
    {
        //arrange
        DependencyContainer container = new();
        container.AddSingleton<IList<int>, List<int>>();
        container.AddSingleton<ServiceH>();

        DependencyResolver resolver = new(container);

        //act
        ServiceH service = Assert.IsType<ServiceH>(resolver.GetService<ServiceH>());

        //assert
        Assert.IsType<List<int>>(service.ListA);
        Assert.Same(service.ListA, service.ListB);
    }
}