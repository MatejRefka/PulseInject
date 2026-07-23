namespace PulseInject.Tests;

public class DependencyTests
{
    [Fact]
    public void Constructor_SetProperties()
    {
        //arrange
        var expectedType = typeof(string);
        var expectedLifetime = DependencyLifetime.Singleton;

        //act
        var dependency = new Dependency(expectedType, expectedLifetime);

        //assert
        Assert.Equal(expectedType, dependency.ImplementationType);
        Assert.Equal(expectedLifetime, dependency.Lifetime);
        Assert.Null(dependency.Instance);
    }

    [Fact]
    public void CacheInstance_SetCacheInstance()
    {
        //arrange
        var dependency = new Dependency(typeof(string), DependencyLifetime.Transient);
        var expectedInstance = "some string instance";

        //act
        dependency.CacheInstance(expectedInstance);

        //assert
        Assert.Equal(expectedInstance, dependency.Instance);
    }

    [Fact]
    public void CacheInstance_ThrowForSettingCacheInstanceTwice()
    {
        //arrange
        var dependency = new Dependency(typeof(string), DependencyLifetime.Singleton);
        dependency.CacheInstance("first instance");

        //act & assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            dependency.CacheInstance("second instance");
        });
    }
}
