using System.Reflection;

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

        private object? GetType(Type type)
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
            var constructor = GetConstructor(type);

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

        //use constructor with the most resolvable parameters
        private ConstructorInfo GetConstructor(Type type)
        {
            var orderedConstructors = type.GetConstructors().OrderByDescending(ctr => ctr.GetParameters().Length);

            foreach (var constructor in orderedConstructors)
            {
                bool parametersResolvable = true;

                foreach (var parameter in constructor.GetParameters())
                {
                    //value types without default value cannot be instantiated
                    if (parameter.ParameterType.IsValueType && !parameter.HasDefaultValue)
                    {
                        parametersResolvable = false;
                        break;
                    }
                    //dependency not registered
                    if (_container.GetDependency(parameter.ParameterType) == null)
                    {
                        parametersResolvable = false;
                        break;
                    }
                }

                if (parametersResolvable)
                {
                    return constructor;
                }
            }
            throw new NotImplementedException($"No suitable constructor found for {type.Name}");
        }
    }
}
