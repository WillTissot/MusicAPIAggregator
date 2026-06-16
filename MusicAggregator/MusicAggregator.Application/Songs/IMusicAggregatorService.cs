using MusicAggregator.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAggregator.Application.Songs
{
    public interface IMusicAggregatorService
    {
        Task<SongPage> GetSongFullInfoAsync(string track, string artist, CancellationToken ct);
        Task<SearchResult> SearchAsync(SongSearchRequest request, CancellationToken ct);
    }
}
