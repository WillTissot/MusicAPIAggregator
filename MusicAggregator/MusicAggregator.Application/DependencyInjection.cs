using Microsoft.Extensions.DependencyInjection;
using MusicAggregator.Application.Songs;

namespace MusicAggregator.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddTransient<IGetSongFullInfo, GetSongFullInfo>();
            return services;
        }
    }
}
