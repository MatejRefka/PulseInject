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

        private object? GetType(Type type, object? declaredDefault = null, Dictionary<Type, object?>? instantiationCache = null)
        {
            var isPrimitiveLike = type.IsValueType || type == typeof(string);

            //holds all instances of the entire resolution tree
            instantiationCache ??= new Dictionary<Type, object?>();

            if (!isPrimitiveLike)
            {
                if (instantiationCache.TryGetValue(type, out var cachedInstance))
                {
                    if (cachedInstance == null)
                    {
                        throw new InvalidOperationException($"Circular dependency for type {type}");
                    }
                    return cachedInstance;
                }
            }

            //placeholder to prevent circular dependency (resolution is depth first)
            instantiationCache[type] = null;

            //get registered dependency
            var dependency = _container.GetDependency(type);

            //singleton instance already instantiated
            if (dependency?.Instance != null && dependency.Lifetime == DependencyLifetime.Singleton)
            {
                instantiationCache[type] = dependency.Instance;
                return dependency.Instance;
            }

            if (isPrimitiveLike)
            {
                //not registered in DI container
                if (dependency == null)
                {
                    if (declaredDefault != null)
                    {
                        //use declared default value
                        return declaredDefault;
                    }
                    else
                    {
                        throw new InvalidOperationException("No declared default value and no registered value for value type or string");
                    }
                }
                else
                {
                    if (dependency.Lifetime == DependencyLifetime.Singleton)
                    {
                        dependency.CacheInstance(dependency.RegisteredValue!);
                    }
                    //use registered value
                    return dependency.RegisteredValue;
                }
            }

            //reference type not registered in DI container
            if (dependency == null)
            {
                throw new InvalidOperationException($"Service of type {type.Name} is not registered");
            }

            //service can have further dependencies passed into its constructor
            var constructor = GetConstructor(type);

            var parameters = constructor.GetParameters();

            //holds instances for the current constructor in context
            var parameterInstances = new List<object>();

            foreach (var parameter in parameters)
            {
                //recursively resolve the parameters of the parameter
                var instance = GetType(parameter.ParameterType, parameter.DefaultValue, instantiationCache);
                if (instance != null)
                {
                    parameterInstances.Add(instance);
                }
            }

            //create service instance with or without params
            var serviceInstance = (parameterInstances.Count > 0)
                ? Activator.CreateInstance(dependency.Type, parameterInstances.ToArray())
                : Activator.CreateInstance(dependency.Type);

            //cache the singleton instance
            if (dependency.Lifetime == DependencyLifetime.Singleton)
            {
                dependency.CacheInstance(serviceInstance!);
            }

            instantiationCache[type] = serviceInstance;
            return serviceInstance;
        }

        //return constructor with the most resolvable parameters
        private ConstructorInfo GetConstructor(Type type)
        {
            var orderedConstructors = type.GetConstructors().OrderByDescending(ctr => ctr.GetParameters().Length);

            foreach (var constructor in orderedConstructors)
            {
                bool allParametersResolvable = true;

                foreach (var parameter in constructor.GetParameters())
                {
                    bool isPrimitiveLike = parameter.ParameterType.IsValueType || parameter.ParameterType == typeof(string);

                    //primitiveLike types without declared default value
                    if (isPrimitiveLike && !parameter.HasDefaultValue)
                    {
                        //plus it's not been registered in DI container
                        if (_container.GetDependency(parameter.ParameterType) == null)
                        {
                            allParametersResolvable = false;
                            break;
                        }
                    }
                    //reference type not registered in DI container
                    if (!isPrimitiveLike && _container.GetDependency(parameter.ParameterType) == null)
                    {
                        allParametersResolvable = false;
                        break;
                    }
                }

                if (allParametersResolvable)
                {
                    return constructor;
                }
            }
            throw new NotImplementedException($"No suitable constructor found for {type.Name}");
        }
    }
}
