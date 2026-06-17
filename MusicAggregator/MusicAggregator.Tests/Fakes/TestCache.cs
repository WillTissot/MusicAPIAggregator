using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace MusicAggregator.Tests.Fakes
{
    public static class TestCache
    {
        public static HybridCache Create()
        {
            var services = new ServiceCollection();
            services.AddHybridCache();  
            return services.BuildServiceProvider().GetRequiredService<HybridCache>();
        }
    }
}
