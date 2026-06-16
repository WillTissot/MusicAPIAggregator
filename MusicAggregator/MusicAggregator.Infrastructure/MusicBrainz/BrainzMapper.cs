using MusicAggregator.Application.Models;
using MusicAggregator.Infrastructure.MusicBrainz.Models;

namespace MusicAggregator.Infrastructure.MusicBrainz
{
    internal static class BrainzMapper
    {
        private const int MaxTags = 5;
        public static ArtistInfo ToArtistInfo(this BrainzArtist a) => new()
        {
            Name = a.Name,
            Country = a.Country,
            Type = a.Type,
            FormedYear = a.LifeSpan?.Begin,
            Tags = (a.Tags ?? [])
                .Where(t => t.Count > 0)  //tags have something like most voted. We should filter out unpopular tags, otherwise we can get a lot of irrelevant tags with 0 votes
                .OrderByDescending(t => t.Count)      // we should order the tags by most voted.
                .Take(MaxTags)
                .Select(t => t.Name)
                .ToList(),
        };
    }
}
