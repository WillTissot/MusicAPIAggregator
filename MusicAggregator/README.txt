#MUSIC API AGGREGATOR

A .NET 9 / ASP.NET Core service that aggregates music data from three external
APIs into a single, unified interface. It fetches in parallel, degrades
gracefully when an upstream fails, caches aggressively, and exposes request
statistics, with  JWT authentication and a background performance
monitor.

#PRODIVERS

  Deezer       Tracks    Track search and track metadata
  MusicBrainz  Artists   Artist search and artist metadata
  LRCLIB       Lyrics    Song lyrics (detail endpoint only)

#ARCHITECTURE


MusicAggregator.Api             controllers, middleware, DI composition root, auth wiring
MusicAggregator.Application     interfaces (abstractions), domain models, orchestration service
MusicAggregator.Infrastructure  external API clients, caching, stats store, background service
MusicAggregator.Tests           unit and integration tests


#TECH STACK

  .NET 9 / ASP.NET Core
  Microsoft.Extensions.Http.Resilience  retries, backoff, timeouts, circuit breaker
  HybridCache                           in-memory caching with stampede protection
  JWT Bearer                            optional authentication
  xUnit + FluentAssertions              testing   (VERIFY test framework versions)


#GETTING STARTED

Prerequisites:
  .NET 9 SDK (https://dotnet.microsoft.com/download)

Run:
  git clone <repo url>
  cd MusicAggregator
  dotnet restore
  dotnet run --project MusicAggregator.Api

The API starts on https://localhost:7096 (and http://localhost:5204).
check MusicAggregator.Api/Properties/launchSettings.json if the above ports are not working.


#CONFIGURTION

Please create your own key for the Jwt logic. On the appsettings.json you can find the config for the aut. For security reasons the key is not provided.

JWT options:

  {
    "Jwt": {
      "Issuer": "MusicAggregator",
      "Audience": "MusicAggregatorClients",
      "Key": "",
      "ExpiryMinutes": 60
    }
  }

You can follow the next steps to generate a safe key.

  cd MusicAggregator.Api
  dotnet user-secrets init
  dotnet user-secrets set "Jwt:Key" "<random-key>"

Generate a key with PowerShell:

  $bytes = New-Object byte[] 32
  [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
  [Convert]::ToBase64String($bytes)

In production, supply Jwt__Key via an environment variable or secret store. 

#API ENDPOINTS

All endpoints are versioned under /api/v1.

Authentication: POST /api/v1/auth/token
Exchanges credentials for a JWT. AllowAnonymous.

  Request:
    { "username": "test", "password": "test" }

  Response 200:
    { "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." }

  401 on bad credentials.


Detail: GET /api/v1/MusicAggregator/song
Full single-song enrichment merged from all three upstreams (Deezer +
MusicBrainz + LRCLIB lyrics). Requires a Bearer token if auth is enabled.

  Query parameters:
    artist   required   Artist name
    track    required   Track title

  Example:
    GET /api/v1/MusicAggregator/song?artist=Radiohead&track=Creep
    Authorization: Bearer <token>

Returns a SongPage (track metadata + artist info + lyrics). If one upstream is
down, the response is still returned with that section empty plus a warning
(graceful degradation).

Search: GET /api/v1/MusicAggregator/search
Returns filterable and sortable lists of tracks and artists (Deezer +
MusicBrainz, parallel). No lyrics. Requires a Bearer token if auth is enabled.

  Query parameters:
    query                       required   Search term
    explicit                    tracks     true/false filter
    minDuration                 tracks     Duration range (seconds)
    country                     artists    e.g. GB


  
  Example:
    GET /api/v1/MusicAggregator/search?query=Radiohead&explicit=false&sortTracks=Duration
    Authorization: Bearer <token>

  Response shape:
    {
      "query": "Radiohead",
      "tracks":  [ TrackInfo ],
      "artists": [ ArtistInfo ],
      "sources": { "tracks": true, "artists": true, "warnings": [] }
    }


Statistics: GET /api/v1/Stats
 Authorization: Bearer <token>
Per-API request statistics, collected by a DelegatingHandler that times each
real upstream call (cache hits are excluded).

  Response shape:
    [
      {
        "api": "Deezer",
        "totalRequests": 42,
        "averageResponseMs": 88.4,
        "buckets": { "fast": 30, "average": 9, "slow": 3 }
      }
    ]

Buckets: fast < 100 ms, average 100-150 ms, slow > 150 ms. The store is a
thread-safe singleton (ConcurrentDictionary + Interlocked).



#AUTHENTICATION USAGE (Postman / curl)

  1. POST /api/v1/auth/token with { "username": "test", "password": "test" },
     then copy the token.
  2. Send it on protected requests:

     curl -H "Authorization: Bearer <token>" \
       "https://localhost:7096/api/v1/MusicAggregator/song?artist=Radiohead&track=Creep"

Tokens expire after 60 minutes.

#EXTRA FEATURES WORTH MENTIONING

Parallelism:
Concurrency used when making the request to providers with Task.WhenAll().

Resilience and graceful degradation:
  - Microsoft.Extensions.Http.Resilience: transient-only retry with exponential
    backoff and jitter, per-attempt and total timeouts, and a circuit breaker,
    per provider.
  - SafeCallAsync: With this method the complete request does not fail if a subsequent request fails. We just send back the warnings.

Error handling:
A global IExceptionHandler handles the following with: 
ProblemDetails (400 / 401 / 404 / 500).

Caching:
HybridCache was used for caching. Redis would be an overkill for this small implementation. HybridCache offers the possibility to set L2 (except L1 which is in memory).
Hybrid Cache also handles better when with a cold cache we get 10 same requests on the same time.

Background performance monitor:
PerformanceMonitor (BackgroundService) runs every 15 minutes (PeriodicTimer),
reads the stats snapshot, and logs a warning when an API's slow-request ratio
is at least 50% (over a minimum sample of 10 requests). 


#TESTING

  dotnet test

Unit tests:
  - Provider clients (Deezer / MusicBrainz / LRCLIB): stub HttpMessageHandler
    plus captured JSON.
  - TokenService: issues a token that validates against the same parameters;
    wrong key fails.

Integration tests (WebApplicationFactory):
  - Protected endpoint without a token -> 401.
  - Login with bad credentials -> 401.
  - Login, then use token -> not 401 (full round trip).

#GIT

Check branches to see the progress.

