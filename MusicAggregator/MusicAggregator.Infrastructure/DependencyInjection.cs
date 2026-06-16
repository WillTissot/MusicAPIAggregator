using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MusicAggregator.Application.Abstractions;
using MusicAggregator.Infrastructure.Deezer;

namespace MusicAggregator.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddOptions<DeezerOptions>()
                .Bind(config.GetSection(DeezerOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddHttpClient<ITrackProvider, DeezerClient>((sp, http) =>
            {
                var o = sp.GetRequiredService<IOptions<DeezerOptions>>().Value;
                http.BaseAddress = new Uri(o.BaseUrl);
                http.Timeout = TimeSpan.FromSeconds(o.TimeoutSeconds);
            });

            return services;
        }
    }
}
