using MusicAggregator.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAggregator.Application.Songs
{
    internal interface IGetSongFullInfo
    {
        Task<SongPage> GetSongFullInfoAsync(string track, string artist, CancellationToken ct);
    }
}
