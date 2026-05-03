namespace LambdaPulse.Engine.DI;

public sealed class DependencyContainer
{
    private readonly List<Dependency> _dependencies = [];

    public void AddSingleton<T>()
    {
        var type = typeof(T);
        var isPrimitiveLike = type.IsValueType || type == typeof(string);

        if (isPrimitiveLike)
        {
            throw new InvalidOperationException("Cannot register value type without an explicitly declared value.");
        }

        _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Singleton));
    }
    public void AddSingleton<T>(T value)
    {
        var type = typeof(T);
        var isPrimitiveLike = type.IsValueType || type == typeof(string);

        if (isPrimitiveLike)
        {
            _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Singleton, registeredValue: value));
        }
        else
        {
            _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Singleton, instance: value));
        }
    }
    public void AddSingleton<TAbstraction, TImplementation>()
        where TImplementation : TAbstraction
    {
        //avoid inheritance of concrete types
        if (!typeof(TAbstraction).IsInterface && !typeof(TAbstraction).IsAbstract)
        {
            throw new InvalidOperationException("TAbstraction must be an interface or an abstract class.");
        }

        _dependencies.Add(new Dependency(typeof(TImplementation), DependencyLifetime.Singleton, abstractType: typeof(TAbstraction)));
    }

    public void AddScoped<T>()
    {
        var type = typeof(T);
        var isPrimitiveLike = type.IsValueType || type == typeof(string);

        if (isPrimitiveLike)
        {
            throw new InvalidOperationException("Cannot register value type without an explicitly declared value.");
        }

        _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Scoped));
    }
    public void AddScoped<T>(T registeredValue)
    {
        var type = typeof(T);
        var isPrimitiveLike = type.IsValueType || type == typeof(string);

        if (!isPrimitiveLike)
        {
            throw new InvalidOperationException("Cannot register reference types by instance.");
        }

        _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Scoped, registeredValue: registeredValue));
    }
    public void AddScoped<TAbstraction, TImplementation>()
        where TImplementation : TAbstraction
    {
        //avoid inheritance of concrete types
        if (!typeof(TAbstraction).IsInterface && !typeof(TAbstraction).IsAbstract)
        {
            throw new InvalidOperationException("TAbstraction must be an interface or an abstract class.");
        }

        _dependencies.Add(new Dependency(typeof(TImplementation), DependencyLifetime.Scoped, abstractType: typeof(TAbstraction)));
    }
    public void AddTransient<T>()
    {
        var type = typeof(T);
        var isPrimitiveLike = type.IsValueType || type == typeof(string);

        if (isPrimitiveLike)
        {
            throw new InvalidOperationException("Cannot register value type without an explicitly declared value.");
        }

        _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Transient));
    }
    public void AddTransient<T>(T registeredValue)
    {
        var type = typeof(T);
        var isPrimitiveLike = type.IsValueType || type == typeof(string);

        if (!isPrimitiveLike)
        {
            throw new InvalidOperationException("Cannot register reference types by instance.");
        }

        _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Transient, registeredValue: registeredValue));
    }
    public void AddTransient<TAbstraction, TImplementation>()
        where TImplementation : TAbstraction
    {
        //avoid inheritance of concrete types
        if (!typeof(TAbstraction).IsInterface && !typeof(TAbstraction).IsAbstract)
        {
            throw new InvalidOperationException("TAbstraction must be an interface or an abstract class.");
        }

        _dependencies.Add(new Dependency(typeof(TImplementation), DependencyLifetime.Transient, abstractType: typeof(TAbstraction)));
    }

    public Dependency? GetDependency(Type type)
    {
        if (type.IsInterface || type.IsAbstract)
        {
            return _dependencies.LastOrDefault(d => d.AbstractType == type);
        }
        else
        {
            return _dependencies.LastOrDefault(d => d.ImplementationType == type);
        }
    }

    public void OverrideSingleton<T>()
    {
        var type = typeof(T);
        _dependencies.RemoveAll(d => d.AbstractType == type || (d.AbstractType == null && d.ImplementationType == type));

        AddSingleton<T>();
    }
    public void OverrideSingleton<T>(T value)
    {
        var type = typeof(T);
        _dependencies.RemoveAll(d => d.AbstractType == type || (d.AbstractType == null && d.ImplementationType == type));

        AddSingleton<T>(value);
    }
    public void OverrideSingleton<TAbstraction, TImplementation>() where TImplementation : TAbstraction
    {
        var type = typeof(TAbstraction);

        _dependencies.RemoveAll(d => d.AbstractType == type || (d.AbstractType == null && d.ImplementationType == type));
        AddSingleton<TAbstraction, TImplementation>();
    }

    public void OverrideScoped<T>()
    {
        var type = typeof(T);
        _dependencies.RemoveAll(d => d.AbstractType == type || (d.AbstractType == null && d.ImplementationType == type));

        AddScoped<T>();
    }
    public void OverrideScoped<T>(T value)
    {
        var type = typeof(T);
        _dependencies.RemoveAll(d => d.AbstractType == type || (d.AbstractType == null && d.ImplementationType == type));

        AddScoped<T>(value);
    }
    public void OverrideScoped<TAbstraction, TImplementation>() where TImplementation : TAbstraction
    {
        var type = typeof(TAbstraction);

        _dependencies.RemoveAll(d => d.AbstractType == type || (d.AbstractType == null && d.ImplementationType == type));
        AddScoped<TAbstraction, TImplementation>();
    }

    public void OverrideTransient<T>()
    {
        var type = typeof(T);
        _dependencies.RemoveAll(d => d.AbstractType == type || (d.AbstractType == null && d.ImplementationType == type));

        AddTransient<T>();
    }
    public void OverrideTransient<T>(T value)
    {
        var type = typeof(T);
        _dependencies.RemoveAll(d => d.AbstractType == type || (d.AbstractType == null && d.ImplementationType == type));

        AddTransient<T>(value);
    }
    public void OverrideTransient<TAbstraction, TImplementation>() where TImplementation : TAbstraction
    {
        var type = typeof(TAbstraction);

        _dependencies.RemoveAll(d => d.AbstractType == type || (d.AbstractType == null && d.ImplementationType == type));
        AddTransient<TAbstraction, TImplementation>();
    }
}
