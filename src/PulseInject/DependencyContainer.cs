namespace LambdaPulse.DI
{
    public class DependencyContainer
    {
        private readonly List<Dependency> _dependencies = [];

        public void AddSingleton<T>()
        {
            var type = typeof(T);
            var isValueTypeOrString = type.IsValueType || type == typeof(string);

            if (isValueTypeOrString)
            {
                throw new InvalidOperationException("Cannot register value type without an explicitly declared value");
            }

            _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Singleton));
        }
        public void AddSingleton<T>(T param)
        {
            var type = typeof(T);
            var isValueTypeOrString = type.IsValueType || type == typeof(string);

            if (isValueTypeOrString)
            {
                _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Singleton, param));
            }
            else
            {
                throw new InvalidOperationException("Cannot register reference types by instance.");
            }
        }
        public void AddScoped<T>()
        {
            var type = typeof(T);
            var isValueTypeOrString = type.IsValueType || type == typeof(string);

            if (isValueTypeOrString)
            {
                throw new InvalidOperationException("Cannot register value type without an explicitly declared value");
            }

            _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Scoped));
        }
        public void AddScoped<T>(T param)
        {
            var type = typeof(T);
            var isValueTypeOrString = type.IsValueType || type == typeof(string);

            if (isValueTypeOrString)
            {
                _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Scoped, param));
            }
            else
            {
                throw new InvalidOperationException("Cannot register reference types by instance.");
            }
        }
        public void AddTransient<T>()
        {
            var type = typeof(T);
            var isValueTypeOrString = type.IsValueType || type == typeof(string);

            if (isValueTypeOrString)
            {
                throw new InvalidOperationException("Cannot register value type without an explicitly declared value");
            }

            _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Transient));
        }
        public void AddTransient<T>(T param)
        {
            var type = typeof(T);
            var isValueTypeOrString = type.IsValueType || type == typeof(string);

            if (isValueTypeOrString)
            {
                _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Transient, param));
            }
            else
            {
                throw new InvalidOperationException("Cannot register reference types by instance.");
            }
        }

        public Dependency? GetDependency(Type type)
        {
            return _dependencies.LastOrDefault(d => d.Type == type);
        }
    }
}
