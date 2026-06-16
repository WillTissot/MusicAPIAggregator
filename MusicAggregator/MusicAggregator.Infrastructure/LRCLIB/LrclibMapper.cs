using MusicAggregator.Application.Models;
using MusicAggregator.Infrastructure.LRCLIB.Models;

namespace MusicAggregator.Infrastructure.LRCLIB
{
    internal static class LrclibMapper
    {
        public static LyricsInfo ToLyricsInfo(this LrclibResponse r) => new()
        {
            PlainLyrics = r.PlainLyrics,
            SyncedLyrics = r.SyncedLyrics,
            IsInstrumental = r.Instrumental,
        };
    }
}
