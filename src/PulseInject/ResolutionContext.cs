namespace LambdaPulse.DI
{
    public class ResolutionContext
    {
        public bool IsTopLevelType { get; set; } = true;
        public bool RequestedBySingleton { get; set; } = false;
        public object? DeclaredDefault { get; set; }
        public Dictionary<Type, object?>? InstantiationCache { get; set; }
    }
}
