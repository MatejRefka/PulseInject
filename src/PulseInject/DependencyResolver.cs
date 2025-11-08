using System.Reflection;

namespace LambdaPulse.DI;

public sealed class DependencyResolver
{
    private readonly DependencyContainer _container;

    public DependencyResolver(DependencyContainer container)
    {
        _container = container;
    }

    public T? GetService<T>()
    {
        //context at each dependency of the resolution tree
        var contex = new ResolutionContext();

        return (T?)GetType(typeof(T), contex);
    }

    private object? GetType(Type type, ResolutionContext context)
    {
        var requestedType = type;
        var isPrimitiveLike = type.IsValueType || type == typeof(string);
        var isAbstractType = type.IsInterface || type.IsAbstract;
        var dependency = _container.GetDependency(type);

        //prevent transient or scoped dependencies within a singleton service
        if (context.RequestedBySingleton && dependency != null && dependency?.Lifetime != DependencyLifetime.Singleton)
        {
            throw new InvalidOperationException($"Cannot register {type} of {dependency!.Lifetime} life into Singleton dependency graph");
        }

        //all dependencies within this dependency graph must be singleton
        if (dependency?.Lifetime == DependencyLifetime.Singleton)
        {
            context.RequestedBySingleton = true;
        }

        //holds instances of the entire resolution tree
        context.InstantiationCache ??= new Dictionary<Type, object?>();

        if (!isPrimitiveLike)
        {
            //instance is fully instantiated
            if (context.InstantiationCache.TryGetValue(type, out var cachedInstance))
            {
                if (cachedInstance == null)
                {
                    throw new InvalidOperationException($"Circular dependency for type {type}");
                }
                return cachedInstance;
            }
            //placeholder to prevent circular dependency (resolution is depth first)
            context.InstantiationCache[type] = null;
        }

        //singleton instance already instantiated
        if (dependency?.Instance != null && dependency.Lifetime == DependencyLifetime.Singleton)
        {
            context.InstantiationCache[type] = dependency.Instance;
            return dependency.Instance;
        }

        if (isPrimitiveLike)
        {
            //not registered in DI container
            if (dependency == null)
            {
                if (context.DeclaredDefault != null)
                {
                    //use declared default value
                    return context.DeclaredDefault;
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

        if (isAbstractType)
        {
            //abstract type not registered in DI container
            if (dependency == null)
            {
                throw new InvalidOperationException($"Service of type {type.Name} is not registered");
            }
            //resolve the implementation
            type = dependency.ImplementationType;
        }
        else if (context.IsTopLevelType)
        {
            //top-level concrete type not registered in DI container
            if (dependency == null)
            {
                throw new InvalidOperationException($"Top-level concrete type {type.Name} is not registered");
            }
        }

        //service can have further dependencies passed into its constructor
        var constructor = GetConstructor(type);

        var parameters = constructor.GetParameters();

        //holds instances for the current constructor in context
        var parameterInstances = new List<object>();

        foreach (var parameter in parameters)
        {
            context.IsTopLevelType = false;
            context.DeclaredDefault = parameter.DefaultValue;

            //recursively resolve the parameters of the parameter
            var instance = GetType(parameter.ParameterType, context);
            if (instance != null)
            {
                parameterInstances.Add(instance);
            }
        }

        //create service instance with or without params
        var serviceInstance = parameterInstances.Count > 0
            ? Activator.CreateInstance(type, parameterInstances.ToArray())
            : Activator.CreateInstance(type);

        //cache the singleton instance
        if (dependency != null)
        {
            if (dependency.Lifetime == DependencyLifetime.Singleton)
            {
                dependency.CacheInstance(serviceInstance!);
            }
        }

        //cache instance under abstraction and implementation types
        context.InstantiationCache[type] = serviceInstance;
        if (requestedType != type)
        {
            context.InstantiationCache[requestedType] = serviceInstance;
        }
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
                bool isAbstractType = parameter.ParameterType.IsInterface || parameter.ParameterType.IsAbstract;

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
                //abstract type not registered in DI container
                if (isAbstractType && _container.GetDependency(parameter.ParameterType) == null)
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
