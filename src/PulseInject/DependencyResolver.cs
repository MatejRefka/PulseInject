namespace LambdaPulse.DI
{
    public class DependencyResolver
    {
        private readonly DependencyContainer _container;

        public DependencyResolver(DependencyContainer container)
        {
            _container = container;
        }

        public T? GetService<T>()
        {
            return (T?)GetType(typeof(T));
        }

        public object? GetType(Type type)
        {
            var dependency = _container.GetDependency(type);

            //service is not registered in the dependency container
            if (dependency == null)
            {
                return default;
            }

            //singleton instance already instantiated
            if (dependency.Instance != null && dependency.Lifetime == DependencyLifetime.Singleton)
            {
                return dependency.Instance;
            }

            //service can have further dependencies passed into its constructor
            var constructors = type.GetConstructors();

            //use constructor with most params
            var constructor = type.GetConstructors().OrderByDescending(ctr => ctr.GetParameters().Length).First();
            var parameters = constructor.GetParameters();

            //holds the instantiated dependency params
            var parameterInstances = new List<object>();

            foreach (var parameter in parameters)
            {
                //instantiate the parameter (dependency of the dependency)
                var instance = GetType(parameter.ParameterType);
                if (instance != null)
                {
                    parameterInstances.Add(instance);
                }
            }

            //create service instance with or without params
            var masterInstance = (parameterInstances.Count > 0)
                ? Activator.CreateInstance(dependency.Type, parameterInstances.ToArray())
                : Activator.CreateInstance(dependency.Type);

            //cache the singleton instance
            if (dependency.Lifetime == DependencyLifetime.Singleton)
            {
                dependency.CacheInstance(masterInstance!);
            }

            return masterInstance;
        }
    }
}
