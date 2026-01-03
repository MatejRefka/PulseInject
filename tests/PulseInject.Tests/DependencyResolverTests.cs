using LambdaPulse.Server.DI;

namespace LambdaPulse.Tests.Core.DI;

public class DependencyResolverTests
{

    #region Service implementations
    public interface IUploadService
    {
        public void UploadFile();
    }

    public class DBUploadService : IUploadService
    {
        private readonly ConsoleLogger _logger;
        public int DBUploadInt { get; }

        public DBUploadService(ConsoleLogger logger, int i = 1)
        {
            _logger = logger;
            DBUploadInt = i;
        }

        public void UploadFile()
        {
            Console.WriteLine($"Uploaded to DB");
            _logger?.Log("Logged successful upload");
        }
        public int GetLoggerInt()
        {
            return _logger.LoggerInt;
        }
        public int GetConfigInt()
        {
            return _logger.GetConfigInt();
        }
    }

    public class AzureUploadService : IUploadService
    {
        private readonly ConsoleLogger _logger;

        public AzureUploadService(ConsoleLogger logger)
        {
            _logger = logger;
        }

        public void UploadFile()
        {
            Console.WriteLine($"Uploaded to Azure");
            _logger?.Log("Logged successful upload");
        }
    }

    public class ConsoleLogger
    {
        private readonly ConfigurationManager _configurationManager;
        public int LoggerInt { get; }
        public ConsoleLogger(ConfigurationManager configurationManager, int i = 2)
        {
            _configurationManager = configurationManager;
            LoggerInt = i;
        }
        public void Log(string message)
        {
            Console.WriteLine($"{message} println into console.");
            Console.WriteLine($"Config string: {_configurationManager.ConfigString}");
        }
        public int GetConfigInt()
        {
            return _configurationManager.ConfigInt;
        }
    }
    public class DBLogger
    {
        private readonly ConfigurationManager _configurationManager;

        public DBLogger(ConfigurationManager configurationManager, int i = 3)
        {
            _configurationManager = configurationManager;
        }
        public void Log(string message)
        {
            Console.WriteLine($"{message} logged into DB.");
            Console.WriteLine($"Config string: {_configurationManager.ConfigString}");
        }
    }

    public class ConfigurationManager
    {
        public int ConfigInt { get; }
        public string ConfigString { get; }
        public bool ConfigBool { get; }

        public ConfigurationManager(int param1 = 12, string param2 = "param2")
        {
            ConfigInt = param1;
            ConfigString = param2;
        }
        public ConfigurationManager(int param1 = 12, string param2 = "param2", bool param3 = true)
        {
            ConfigInt = param1;
            ConfigString = param2;
            ConfigBool = param3;
        }
        public ConfigurationManager(int pram0, int param1 = 1, string param2 = "2", bool param3 = false)
        {
            ConfigInt = param1;
            ConfigString = param2;
            ConfigBool = param3;
        }
    }

    public class ServiceA
    {
        public ServiceA(ServiceB serviceB) { }
    }

    public class ServiceB
    {
        public ServiceB(ServiceA serviceA) { }
    }

    public interface IConfigProvider { }
    public class ConfigProvider : IConfigProvider { }
    public class ConfigConsumerA
    {
        public ConfigConsumerA(ConfigConsumerB consumerB, IConfigProvider provider) { }
    }
    public class ConfigConsumerB
    {
        public ConfigConsumerB(IConfigProvider provider) { }
    }

    #endregion Service implementations

    //TEST 1
    [Fact]
    public void GetType_ResolveRegisteredValue()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddSingleton<bool>(true);
        container.AddSingleton<string>("string1");
        container.AddSingleton("string2");

        var resolver = new DependencyResolver(container);

        //act
        var boolService = resolver.GetService<bool>();
        var stringService = resolver.GetService<string>();

        //assert
        Assert.True(boolService);
        Assert.Equal("string2", stringService);
    }

    //TEST 2
    [Fact]
    public void GetType_ThrowForUndeclaredDefault()
    {
        //arrange
        var container = new DependencyContainer();

        var resolver = new DependencyResolver(container);

        //act & assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            var service = resolver.GetService<int>();
        });
    }

    //TEST 3
    [Fact]
    public void GetType_ResolveDeclaredDefault()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddSingleton<ConfigurationManager>();

        var resolver = new DependencyResolver(container);

        //act
        var service = resolver.GetService<ConfigurationManager>();

        //assert
        Assert.NotNull(service);
        Assert.Equal(12, service.ConfigInt);
        Assert.Equal("param2", service.ConfigString);
        Assert.True(service.ConfigBool);
    }

    //TEST 4
    [Fact]
    public void GetType_ThrowForUnregisteredTopLevelConcreteType()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddSingleton<DBUploadService>();

        var resolver = new DependencyResolver(container);

        //act & assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            var service = resolver.GetService<ConfigurationManager>();
        });
    }

    //TEST 4.5
    [Fact]
    public void GetType_ResolveUnregisteredConcreteTypeAsDependency()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddSingleton<DBUploadService>();

        var resolver = new DependencyResolver(container);

        //act
        var service = resolver.GetService<DBUploadService>();

        //assert
        Assert.NotNull(service);
    }

    //TEST 5
    [Fact]
    public void GetType_GetCachedSingletonInstance()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddSingleton<ConfigurationManager>();
        var dependency = container.GetDependency(typeof(ConfigurationManager));

        var resolver = new DependencyResolver(container);

        //act
        var service = resolver.GetService<ConfigurationManager>();

        //assert
        Assert.NotNull(service);
        Assert.NotNull(dependency?.Instance);
        Assert.Equal(dependency?.Instance, service);
    }

    //TEST 5.5
    [Fact]
    public void GetType_GetRegisteredReferenceTypeInstance()
    {
        //arrange
        var container = new DependencyContainer();
        var ex = new Exception("Default Exception");
        container.AddSingleton(ex);
        var dependency = container.GetDependency(typeof(Exception));

        var resolver = new DependencyResolver(container);

        //act
        var service = resolver.GetService<Exception>();

        //assert
        Assert.NotNull(service);
        Assert.NotNull(dependency?.Instance);
        Assert.Equal(dependency?.Instance, service);
        Assert.Equal(service, ex);
    }

    //TEST 6
    [Fact]
    public void GetType_RegisteredValueTakesPrecedenceOverDeclaredDefault()
    {
        //arrange
        var container = new DependencyContainer();

        container.AddSingleton<DBUploadService>();
        container.AddSingleton<ConsoleLogger>();
        container.AddSingleton<ConfigurationManager>();
        container.AddSingleton<int>(55);

        var resolver = new DependencyResolver(container);

        //act
        var service = resolver.GetService<DBUploadService>();

        //assert
        Assert.NotNull(service);
        Assert.Equal(55, service.DBUploadInt);
        Assert.Equal(55, service.GetLoggerInt());
        Assert.Equal(55, service.GetConfigInt());
    }

    //TEST 7
    [Fact]
    public void GetType_DetectCircularDependency()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddSingleton<ServiceA>();
        container.AddSingleton<ServiceB>();

        var resolver = new DependencyResolver(container);

        //act & assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            var service = resolver.GetService<ServiceA>();
        });
    }

    //TEST 8
    [Fact]
    public void GetType_ResolveAbstractService()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddSingleton<IUploadService, DBUploadService>();
        container.AddSingleton<IUploadService, AzureUploadService>();

        var resolver = new DependencyResolver(container);

        //act
        var service = resolver.GetService<IUploadService>();

        //assert
        Assert.NotNull(service);
        Assert.IsType<AzureUploadService>(service);
    }

    //TEST 9
    [Fact]
    public void GetType_ThrowForTransientOrScopedDependenciesWithinSingletonService()
    {
        var container = new DependencyContainer();
        container.AddTransient<IUploadService, AzureUploadService>();
        container.AddSingleton<ConsoleLogger>();
        container.AddScoped<ConfigurationManager>();

        var resolver = new DependencyResolver(container);

        //act & assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            var service = resolver.GetService<IUploadService>();
        });
    }

    [Fact]
    public void GetType_ResolveSharedAbstractType()
    {
        //arrange
        var container = new DependencyContainer();
        container.AddSingleton<IConfigProvider, ConfigProvider>();
        container.AddSingleton<ConfigConsumerA>();
        container.AddSingleton<ConfigConsumerB>();

        var resolver = new DependencyResolver(container);

        //act
        var service = resolver.GetService<ConfigConsumerA>();

        //assert
        Assert.NotNull(service);
        Assert.IsType<ConfigConsumerA>(service);
    }
}
