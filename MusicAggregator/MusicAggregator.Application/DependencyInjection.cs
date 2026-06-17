using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicAggregator.Application.Auth;
using MusicAggregator.Application.Songs;

namespace MusicAggregator.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config)
        {
            services.AddTransient<IMusicAggregatorService, MusicAggregatorService>();

            services.AddOptions<JwtOptions>()
                    .Bind(config.GetSection(JwtOptions.SectionName))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

            return services;
        }
    }
}
