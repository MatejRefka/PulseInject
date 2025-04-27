using LambdaPulse.DI;

namespace LambdaPulse.Tests.DI
{
    public class DependencyContainerTests
    {
        [Fact]
        public void AddSingleton_AddsSingletonDependency()
        {
            //arrange
            var container = new DependencyContainer();

            //act
            container.AddSingleton<string>();
            var dependency = container.GetDependency(typeof(string));

            //assert
            Assert.NotNull(dependency);
            Assert.Equal(typeof(string), dependency.Type);
            Assert.Equal(DependencyLifetime.Singleton, dependency.Lifetime);
        }

        [Fact]
        public void AddScoped_AddsScopedDependency()
        {
            //arrange
            var container = new DependencyContainer();

            //act
            container.AddScoped<string>();
            var dependency = container.GetDependency(typeof(string));

            //assert
            Assert.NotNull(dependency);
            Assert.Equal(typeof(string), dependency.Type);
            Assert.Equal(DependencyLifetime.Scoped, dependency.Lifetime);
        }

        [Fact]
        public void AddTransient_AddsTransientDependency()
        {
            //arrange
            var container = new DependencyContainer();

            //act
            container.AddTransient<string>();
            var dependency = container.GetDependency(typeof(string));

            //assert
            Assert.NotNull(dependency);
            Assert.Equal(typeof(string), dependency.Type);
            Assert.Equal(DependencyLifetime.Transient, dependency.Lifetime);
        }

        [Fact]
        public void GetDependency_ReturnsDependencyByType()
        {
            //arrange
            var container = new DependencyContainer();
            container.AddSingleton<string>();
            container.AddTransient<int>();

            //act
            var dependency = container.GetDependency(typeof(int));

            //assert
            Assert.NotNull(dependency);
            Assert.Equal(typeof(int), dependency.Type);
            Assert.Equal(DependencyLifetime.Transient, dependency.Lifetime);
        }
    }
}
