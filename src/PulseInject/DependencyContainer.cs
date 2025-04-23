namespace LambdaPulse.DI
{
    public class DependencyContainer
    {
        private readonly List<Dependency> _dependencies = [];

        public void AddSingleton<T>()
        {
            _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Singleton));
        }
        public void AddScoped<T>()
        {
            _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Scoped));
        }
        public void AddTransient<T>()
        {
            _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Transient));
        }

        public Dependency? GetDependency(Type type)
        {
            return _dependencies.FirstOrDefault(d => d.Type == type);
        }
    }
}
