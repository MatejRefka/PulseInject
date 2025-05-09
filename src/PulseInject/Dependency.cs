namespace LambdaPulse.DI
{
    public class Dependency
    {
        public object? Instance { get; private set; }
        public Type Type { get; }
        public DependencyLifetime Lifetime { get; }
        public object? RegisteredValue { get; }

        public Dependency(Type type, DependencyLifetime lifetime, object? param = null)
        {
            Type = type;
            Lifetime = lifetime;
            RegisteredValue = param;
        }

        public void CacheInstance(object instance)
        {
            if (Instance != null)
            {
                throw new InvalidOperationException($"Instance of type {Type.Name} has already been set.");
            }
            Instance = instance;
        }
    }
}
