
namespace MusicAggregator.Application.Models
{
    public sealed class SourceStatus
    {
        public bool Track { get; init; }
        public bool Artist { get; init; }
        public bool Lyrics { get; init; }

        public IReadOnlyList<string> Warnings { get; init; } = [];
    }
}
