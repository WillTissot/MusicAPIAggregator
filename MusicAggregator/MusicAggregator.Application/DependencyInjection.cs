using Microsoft.Extensions.DependencyInjection;

namespace MusicAggregator.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //add aggregator service.
            return services;
        }
    }
}
