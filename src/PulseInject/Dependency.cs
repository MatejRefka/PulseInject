namespace LambdaPulse.DI
{
    public class Dependency
    {
        public object? Instance { get; private set; }
        public Type Type { get; }
        public DependencyLifetime Lifetime { get; }

        public Dependency(Type type, DependencyLifetime lifetime)
        {
            Type = type;
            Lifetime = lifetime;
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
