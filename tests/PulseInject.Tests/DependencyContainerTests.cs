namespace PulseInject.Tests;

public class DependencyContainerTests
{
    [Fact]
    public void AddSingleton_AddSingletonDependency()
    {
        //arrange
        var container = new DependencyContainer();

        //act
        container.AddSingleton<object>();
        var dependency = container.GetDependency(typeof(object));

        //assert
        Assert.NotNull(dependency);
        Assert.Equal(typeof(object), dependency.ImplementationType);
        Assert.Equal(DependencyLifetime.Singleton, dependency.Lifetime);
    }

    [Fact]
    public void AddScoped_AddScopedDependency()
    {
        //arrange
        var container = new DependencyContainer();

        //act
        container.AddScoped(12);
        var dependency = container.GetDependency(typeof(int));

        //assert
        Assert.NotNull(dependency);
        Assert.Equal(typeof(int), dependency.ImplementationType);
        Assert.Equal(DependencyLifetime.Scoped, dependency.Lifetime);
    }

    [Fact]
    public void AddTransient_AddTransientDependency()
    {
        //arrange
        var container = new DependencyContainer();

        //act
        container.AddTransient("hello");
        var dependency = container.GetDependency(typeof(string));

        //assert
        Assert.NotNull(dependency);
        Assert.Equal(typeof(string), dependency.ImplementationType);
        Assert.Equal(DependencyLifetime.Transient, dependency.Lifetime);
    }

    [Fact]
    public void AddSingleton_AddDeclaredReferenceType()
    {
        //arrange
        var container = new DependencyContainer();
        var e = new Exception("Default exception");

        //act 
        container.AddSingleton(e);
        var dependency = container.GetDependency(typeof(Exception));

        //assert
        Assert.NotNull(dependency);
        Assert.Equal(typeof(Exception), dependency.ImplementationType);
        Assert.Equal(DependencyLifetime.Singleton, dependency.Lifetime);
        Assert.Equal(e, dependency.Instance);
    }

    [Fact]
    public void AddScoped_ThrowsForDeclaredReferenceType()
    {
        //arrange
        var container = new DependencyContainer();

        //act & assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            container.AddScoped(new object());
        });
    }

    [Fact]
    public void AddTransient_ThrowsForDeclaredReferenceType()
    {
        //arrange
        var container = new DependencyContainer();

        //act & assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            container.AddTransient(new object());
        });
    }

    [Fact]
    public void AddSingleton_ThrowsForInheritanceConcreteTypes()
    {
        //arrange
        var container = new DependencyContainer();

        //act & assert
        Assert.Throws<InvalidOperationException>(container.AddSingleton<Exception, InvalidCastException>);
    }

    [Fact]
    public void GetDependency_ReturnDependencyByType()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddTransient(12);
        container.AddScoped<IList<int>, List<int>>();

        //act
        var intService = container.GetDependency(typeof(int));
        var interfaceService = container.GetDependency(typeof(IList<int>));

        //assert
        Assert.NotNull(intService);
        Assert.Null(intService.AbstractType);
        Assert.Equal(typeof(int), intService.ImplementationType);
        Assert.Equal(DependencyLifetime.Transient, intService.Lifetime);

        Assert.NotNull(interfaceService);
        Assert.Equal(typeof(IList<int>), interfaceService.AbstractType);
        Assert.Equal(typeof(List<int>), interfaceService.ImplementationType);
        Assert.Equal(DependencyLifetime.Scoped, interfaceService.Lifetime);
    }

    [Fact]
    public void GetDependency_ResolveLatestRegisteredType()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddSingleton(21);
        container.AddSingleton(12);

        var resolver = new DependencyResolver(container);

        //act
        var intService = resolver.GetService<int>();

        //assert
        Assert.Equal(12, intService);
    }

    [Fact]
    public void OverrideSingleton_OverrideTypeWithItself()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddSingleton<object>();

        //act
        container.OverrideSingleton<object>();
        var dependency = container.GetDependency(typeof(object));

        //assert
        Assert.NotNull(dependency);
        Assert.Equal(typeof(object), dependency.ImplementationType);
        Assert.Equal(DependencyLifetime.Singleton, dependency.Lifetime);
    }

    [Fact]
    public void OverrideSingleton_OverrideTypeWithInstance()
    {
        // arrange
        var container = new DependencyContainer();
        container.AddSingleton<object>();

        var instance = new object();

        // act
        container.OverrideSingleton(instance);
        var dependency = container.GetDependency(typeof(object));

        // assert
        Assert.NotNull(dependency);
        Assert.Equal(typeof(object), dependency.ImplementationType);
        Assert.Equal(DependencyLifetime.Singleton, dependency.Lifetime);
        Assert.Same(instance, dependency.Instance);
    }

    [Fact]
    public void OverrideSingleton_OverrideRegisteredImplementation()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddSingleton<ICollection<int>, List<int>>();

        //act
        container.OverrideSingleton<ICollection<int>, HashSet<int>>();
        var dependency = container.GetDependency(typeof(ICollection<int>));

        //assert
        Assert.NotNull(dependency);
        Assert.Equal(typeof(HashSet<int>), dependency.ImplementationType);
        Assert.Equal(typeof(ICollection<int>), dependency.AbstractType);
        Assert.Equal(DependencyLifetime.Singleton, dependency.Lifetime);
    }
}
