namespace MusicAggregator.Infrastructure.Common
{
    public abstract class ProviderOptions
    {
        public string BaseUrl { get; init; } = "";
        public int TimeoutSeconds { get; init; } = 10;
    }
}
