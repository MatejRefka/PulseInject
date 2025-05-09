using LambdaPulse.DI;

namespace LambdaPulse.Tests.DI
{
    public class DependencyContainerTests
    {
        [Fact]
        public void AddSingleton_AddSingletonDependency()
        {
            //arrange
            var container = new DependencyContainer();

            //act
            container.AddSingleton<string>("hello");
            var dependency = container.GetDependency(typeof(string));

            //assert
            Assert.NotNull(dependency);
            Assert.Equal(typeof(string), dependency.Type);
            Assert.Equal(DependencyLifetime.Singleton, dependency.Lifetime);
        }

        [Fact]
        public void AddScoped_AddScopedDependency()
        {
            //arrange
            var container = new DependencyContainer();

            //act
            container.AddScoped<string>("hello");
            var dependency = container.GetDependency(typeof(string));

            //assert
            Assert.NotNull(dependency);
            Assert.Equal(typeof(string), dependency.Type);
            Assert.Equal(DependencyLifetime.Scoped, dependency.Lifetime);
        }

        [Fact]
        public void AddTransient_AddTransientDependency()
        {
            //arrange
            var container = new DependencyContainer();

            //act
            container.AddTransient<string>("hello");
            var dependency = container.GetDependency(typeof(string));

            //assert
            Assert.NotNull(dependency);
            Assert.Equal(typeof(string), dependency.Type);
            Assert.Equal(DependencyLifetime.Transient, dependency.Lifetime);
        }

        [Fact]
        public void GetDependency_ReturnDependencyByType()
        {
            //arrange
            var container = new DependencyContainer();
            container.AddSingleton<string>("hello");
            container.AddTransient<int>(12);

            //act
            var dependency = container.GetDependency(typeof(int));

            //assert
            Assert.NotNull(dependency);
            Assert.Equal(typeof(int), dependency.Type);
            Assert.Equal(DependencyLifetime.Transient, dependency.Lifetime);
        }

        [Fact]
        public void AddSingleton_ResolveUnregisteredReferenceType()
        {
            //arrange
            var container = new DependencyContainer();

            //act & assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                container.AddSingleton<object>(new object());
            });
        }

        [Fact]
        public void GetDependency_ResolveLatestRegisteredType()
        {
            //arrange
            var container = new DependencyContainer();
            container.AddSingleton<int>(21);
            container.AddSingleton<int>(12);

            var resolver = new DependencyResolver(container);

            //act
            var intService = resolver.GetService<int>();

            //assert
            Assert.Equal(12, intService);
        }
    }
}
