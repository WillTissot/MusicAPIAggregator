using MusicAggregator.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAggregator.Infrastructure.MusicBrainz
{
    public sealed class BrainzOptions : ProviderOptions
    {
        public const string SectionName = "Providers:Brainz";

        [Required]
        public string UserAgent { get; init; } = "";
    }
}
