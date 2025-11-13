namespace LambdaPulse.DI;

public sealed class Dependency
{
    public object? Instance { get; private set; }
    public Type? AbstractType { get; }
    public Type ImplementationType { get; }
    public DependencyLifetime Lifetime { get; }
    public object? RegisteredValue { get; }

    public Dependency(Type implementationType, DependencyLifetime lifetime, Type? abstractType = null, object? registeredValue = null, object? instance = null)
    {
        AbstractType = abstractType;
        ImplementationType = implementationType;
        Lifetime = lifetime;
        RegisteredValue = registeredValue;
        Instance = instance;
    }

    public void CacheInstance(object instance)
    {
        if (Instance != null)
        {
            throw new InvalidOperationException($"Instance of type {ImplementationType.Name} has already been set.");
        }
        Instance = instance;
    }
}
