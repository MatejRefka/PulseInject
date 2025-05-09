using LambdaPulse.DI;

namespace LambdaPulse.Tests.DI
{
    public class DependencyResolverTests
    {

        #region Service implementations
        public class DBUploadService
        {
            private readonly ConsoleLogger? _logger;

            public DBUploadService(ConsoleLogger logger)
            {
                _logger = logger;
            }

            public void UploadFile()
            {
                Console.WriteLine($"Uploaded to DB");
                _logger?.Log("Logged successful upload");
            }
        }

        public class AzureUploadService
        {
            private readonly ConsoleLogger? _logger;

            public AzureUploadService()
            {
            }
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
            public ConsoleLogger(ConfigurationManager configurationManager)
            {
                _configurationManager = configurationManager;
            }
            public void Log(string message)
            {
                Console.WriteLine($"{message} println into console.");
                Console.WriteLine($"Config string: {_configurationManager.ConfigString}");
            }
        }
        public class DBLogger
        {
            private readonly ConfigurationManager _configurationManager;

            public DBLogger(ConfigurationManager configurationManager, int param1 = 12)
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
        public void GetType_ResolveUndeclaredDefault()
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
        public void GetType_ResolveUnregisteredReferenceType()
        {
            //arrange
            var container = new DependencyContainer();
            container.AddSingleton<ConfigurationManager>();

            var resolver = new DependencyResolver(container);

            //act & assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                var service = resolver.GetService<DBUploadService>();
            });
        }

        //TEST 5
        [Fact]
        public void GetType_GetCachedInstance()
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
    }
}
