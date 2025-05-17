namespace LambdaPulse.DI
{
    public class Dependency
    {
        public object? Instance { get; private set; }
        public Type? AbstractType { get; }
        public Type ImplementationType { get; }
        public DependencyLifetime Lifetime { get; }
        public object? RegisteredValue { get; }

        public Dependency(Type implementationType, DependencyLifetime lifetime, Type? abstractType = null, object? registeredValue = null)
        {
            AbstractType = abstractType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
            RegisteredValue = registeredValue;
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
}
