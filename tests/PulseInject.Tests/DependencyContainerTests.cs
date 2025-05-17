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
            container.AddSingleton<object>();
            var dependency = container.GetDependency(typeof(object));

            //assert
            Assert.NotNull(dependency);
            Assert.Equal(typeof(object), dependency.ImplementationType);
            Assert.Equal(DependencyLifetime.Singleton, dependency.Lifetime);
        }

        [Fact]
        public void AddScoped_AddScopedDependency()
        {
            //arrange
            var container = new DependencyContainer();

            //act
            container.AddScoped<int>(12);
            var dependency = container.GetDependency(typeof(int));

            //assert
            Assert.NotNull(dependency);
            Assert.Equal(typeof(int), dependency.ImplementationType);
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
            Assert.Equal(typeof(string), dependency.ImplementationType);
            Assert.Equal(DependencyLifetime.Transient, dependency.Lifetime);
        }

        [Fact]
        public void AddSingleton_ThrowsForDeclaredReferenceType()
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
        public void AddSingleton_ThrowsForInheritanceConcreteTypes()
        {
            //arrange
            var container = new DependencyContainer();

            //act & assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                container.AddSingleton<Exception, InvalidCastException>();
            });
        }


        [Fact]
        public void GetDependency_ReturnDependencyByType()
        {
            //arrange
            var container = new DependencyContainer();
            container.AddTransient<int>(12);
            container.AddScoped<IList<int>, List<int>>();

            //act
            var intService = container.GetDependency(typeof(int));
            var interfaceService = container.GetDependency(typeof(IList<int>));

            //assert
            Assert.NotNull(intService);
            Assert.Null(intService.AbstractType);
            Assert.Equal(typeof(int), intService.ImplementationType);
            Assert.Equal(DependencyLifetime.Transient, intService.Lifetime);

            Assert.NotNull(interfaceService);
            Assert.Equal(typeof(IList<int>), interfaceService.AbstractType);
            Assert.Equal(typeof(List<int>), interfaceService.ImplementationType);
            Assert.Equal(DependencyLifetime.Scoped, interfaceService.Lifetime);
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
