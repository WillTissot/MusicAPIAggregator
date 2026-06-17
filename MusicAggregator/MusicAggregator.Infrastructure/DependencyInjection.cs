using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MusicAggregator.Application.Abstractions;
using MusicAggregator.Application.Auth;
using MusicAggregator.Infrastructure.Auth;
using MusicAggregator.Infrastructure.Deezer;
using MusicAggregator.Infrastructure.LRCLIB;
using MusicAggregator.Infrastructure.MusicBrainz;
using MusicAggregator.Infrastructure.Statistics;

namespace MusicAggregator.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddHybridCache();
            services.AddSingleton<IApiStatsStore, ApiStatsStore>();  
            services.AddSingleton<ITokenService, TokenService>();
            services.AddHostedService<PerformanceMonitor>();
            services.AddTransient<ApiStatsHandler>();          

            services.AddOptions<DeezerOptions>()
                .Bind(config.GetSection(DeezerOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<BrainzOptions>()
                .Bind(config.GetSection(BrainzOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<LrclibOptions>()
                .Bind(config.GetSection(LrclibOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddHttpClient<ITrackProvider, DeezerClient>((sp, http) =>
            {
                var o = sp.GetRequiredService<IOptions<DeezerOptions>>().Value;
                http.BaseAddress = new Uri(o.BaseUrl);
                http.Timeout = Timeout.InfiniteTimeSpan;
            }).AddResilience().AddHttpMessageHandler<ApiStatsHandler>();

            services.AddHttpClient<IArtistProvider, BrainzClient>((sp, http) =>
            {
                var o = sp.GetRequiredService<IOptions<BrainzOptions>>().Value;
                http.BaseAddress = new Uri(o.BaseUrl);
                http.DefaultRequestHeaders.UserAgent.ParseAdd(o.UserAgent);
                http.Timeout = Timeout.InfiniteTimeSpan;
            }).AddResilience().AddHttpMessageHandler<ApiStatsHandler>();

            services.AddHttpClient<ILyricsProvider, LrclibClient>((sp, http) =>
            {
                var o = sp.GetRequiredService<IOptions<LrclibOptions>>().Value;
                http.BaseAddress = new Uri(o.BaseUrl);
                http.Timeout = Timeout.InfiniteTimeSpan;
            }).AddResilience(attemptTimeoutSeconds: 15).AddHttpMessageHandler<ApiStatsHandler>();

            return services;
        }

        private static IHttpClientBuilder AddResilience(this IHttpClientBuilder builder , int attemptTimeoutSeconds = 5, int maxRetries = 3)
        {
            builder.AddStandardResilienceHandler(r => {
                r.Retry.MaxRetryAttempts = 3;
                r.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
                r.Retry.UseJitter = true;
                r.Retry.Delay = TimeSpan.FromSeconds(1);

                r.AttemptTimeout.Timeout = TimeSpan.FromSeconds(attemptTimeoutSeconds);

                r.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(attemptTimeoutSeconds * (maxRetries + 1) + 5);
            });

            return builder;
        }
    }
}
