# Podcast API .NET Library

[![.NET](https://github.com/ListenNotes/podcast-api-dotnet/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ListenNotes/podcast-api-dotnet/actions/workflows/dotnet.yml) [![NuGet](https://img.shields.io/nuget/v/PodcastAPI.svg)](https://www.nuget.org/packages/PodcastAPI/)

The official C# library for the [Listen Notes Podcast API](https://www.listennotes.com/api/).
Search podcasts and episodes, explore recommendations, and manage playlists.
Questions? Contact [hello@listennotes.com](mailto:hello@listennotes.com).

## Installation

Install the [PodcastAPI NuGet package](https://www.nuget.org/packages/PodcastAPI/):

```sh
dotnet add package PodcastAPI
```

Alternatively use `Install-Package PodcastAPI` in Visual Studio's Package Manager
Console, or search for **PodcastAPI** in Manage NuGet Packages.

### Requirements

.NET 8 or later. CI tests .NET 8 and .NET 10. Newtonsoft.Json preserves support
for `ToJSON<dynamic>()`; HTTP requests use the built-in `HttpClient`.

## Usage

Get an API key from your [Listen API dashboard](https://www.listennotes.com/api/dashboard/#apps).
Set `LISTEN_API_KEY` for real requests. A missing, empty, or whitespace-only key
selects the public [mock server](https://help.listennotes.com/en/articles/5224500-how-to-test-the-podcast-api-without-an-api-key).
The mock returns stateless fixtures; writes do not persist.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string> { ["q"] = "startup", ["type"] = "episode" };
var response = await client.Search(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
Console.WriteLine($"HTTP {(int)response.StatusCode}");
Console.WriteLine(response.GetFreeQuota());
Console.WriteLine(response.GetUsage());
Console.WriteLine(response.GetNextBillingDate());
```

Reuse a client across requests and dispose it when finished. Requests have a
30-second timeout and accept an optional `CancellationToken`. Set `timeout:` in
the constructor to change the timeout. Parameters are copied, path identifiers
are encoded, and explicitly empty notes/descriptions are sent unchanged.

`ApiResponse` buffers the JSON and exposes `StatusCode` and case-insensitive
`Headers`. `ToString()` returns the raw JSON; `ToJSON<T>()` deserializes it.
`GetFreeQuota()` and `GetUsage()` throw `InvalidOperationException` if their header
is missing or invalid; `GetNextBillingDate()` returns null if absent.

An optional caller-owned `HttpClient` can be supplied via `httpClient:`. Disposing
the SDK does not dispose that client. Its handler must disable redirects and
retries, and it should not contain a default API key header. Its own timeout also
applies. The default SDK transport disables redirects and cookies and adds no
retries. `baseUrl:` overrides the destination, including where your API key is sent.

### Handling exceptions

All SDK request errors derive from `PodcastAPI.Exceptions.ListenApiException`.
HTTP errors expose the buffered body and headers through `error.Response`.
Transport errors have no response. Caller cancellation remains
`OperationCanceledException`; malformed or missing path parameters raise
`ArgumentException` before sending a request.

| Exception | Meaning |
| --- | --- |
| `InvalidRequestException` | HTTP 400 |
| `AuthenticationException` | HTTP 401 |
| `PermissionDeniedException` | HTTP 403 |
| `NotFoundException` | HTTP 404 |
| `RateLimitException` | HTTP 429 |
| `ApiConnectionException` | Network failure or request timeout |
| `ListenApiException` | Other non-success HTTP status, including redirects |

See the runnable [sample application](src/SampleApp/Sample.cs).

### Migrating from 1.x

- Requires .NET 8+. The 25 existing public method names and parameter-dictionary
  calling convention remain. Five playlist write methods are added.
- RestSharp has been removed. The public `client.restClient` and
  `response.response` fields are removed; use constructor transport options and
  `response.StatusCode` / `response.Headers` instead. `ApiResponse`'s constructor
  now accepts an `HttpResponseMessage` and snapshots it without owning it.
- Use `await` for calls. All methods support cancellation. Reusing a dictionary
  no longer loses its path identifiers. Dispose the SDK client when finished.
- Every non-2xx status now throws, including 403 and redirects. SDK exceptions
  share `ListenApiException` as their base. Redirects are not followed by the
  default transport, and the user agent is `podcast-api-dotnet 3.0.0`.

## Method index

<!-- BEGIN GENERATED METHOD INDEX -->

- [`Search`](#search) — `GET /search`
- [`Typeahead`](#typeahead) — `GET /typeahead`
- [`SearchEpisodeTitles`](#searchepisodetitles) — `GET /search_episode_titles`
- [`SpellCheck`](#spellcheck) — `GET /spellcheck`
- [`FetchRelatedSearches`](#fetchrelatedsearches) — `GET /related_searches`
- [`FetchTrendingSearches`](#fetchtrendingsearches) — `GET /trending_searches`
- [`FetchBestPodcasts`](#fetchbestpodcasts) — `GET /best_podcasts`
- [`FetchPodcastById`](#fetchpodcastbyid) — `GET /podcasts/{id}`
- [`DeletePodcast`](#deletepodcast) — `DELETE /podcasts/{id}`
- [`FetchEpisodeById`](#fetchepisodebyid) — `GET /episodes/{id}`
- [`BatchFetchEpisodes`](#batchfetchepisodes) — `POST /episodes`
- [`BatchFetchPodcasts`](#batchfetchpodcasts) — `POST /podcasts`
- [`FetchCuratedPodcastsListById`](#fetchcuratedpodcastslistbyid) — `GET /curated_podcasts/{id}`
- [`FetchPodcastGenres`](#fetchpodcastgenres) — `GET /genres`
- [`FetchPodcastRegions`](#fetchpodcastregions) — `GET /regions`
- [`FetchPodcastLanguages`](#fetchpodcastlanguages) — `GET /languages`
- [`JustListen`](#justlisten) — `GET /just_listen`
- [`FetchCuratedPodcastsLists`](#fetchcuratedpodcastslists) — `GET /curated_podcasts`
- [`FetchRecommendationsForPodcast`](#fetchrecommendationsforpodcast) — `GET /podcasts/{id}/recommendations`
- [`FetchRecommendationsForEpisode`](#fetchrecommendationsforepisode) — `GET /episodes/{id}/recommendations`
- [`SubmitPodcast`](#submitpodcast) — `POST /podcasts/submit`
- [`FetchPlaylistById`](#fetchplaylistbyid) — `GET /playlists/{id}`
- [`FetchMyPlaylists`](#fetchmyplaylists) — `GET /playlists`
- [`FetchAudienceForPodcast`](#fetchaudienceforpodcast) — `GET /podcasts/{id}/audience`
- [`FetchPodcastsByDomain`](#fetchpodcastsbydomain) — `GET /podcasts/domains/{domain_name}`
- [`CreatePlaylist`](#createplaylist) — `POST /playlists`
- [`UpdatePlaylist`](#updateplaylist) — `PUT /playlists/{id}`
- [`AddPlaylistItem`](#addplaylistitem) — `POST /playlists/{id}/items`
- [`DeletePlaylistItem`](#deleteplaylistitem) — `DELETE /playlists/{id}/items/{item_id}`
- [`UpdatePlaylistItemNotes`](#updateplaylistitemnotes) — `PUT /playlists/{id}/items/{item_id}`

<!-- END GENERATED METHOD INDEX -->

## API reference

<!-- BEGIN GENERATED API REFERENCE -->

All methods accept an optional `IDictionary<string, string>` containing path, query, and body parameters, plus an optional `CancellationToken`. They return `Task<ApiResponse>`. Use `await`; parameters are never modified. Set `LISTEN_API_KEY` for real requests; without it these examples use the stateless mock server.

### Search

Full-text search

`GET /search`

Full-text search on episodes, podcasts, or curated lists of podcasts.
Use the `offset` parameter to paginate through search results.
The FREE plan allows to see up to 30 search results (or `offset` < 30) per query.
The PRO plan allows to see up to 300 search results (or `offset` < 300) per query.
The ENTERPRISE plan allows to see up to 10,000 search results (or `offset` < 10000) per query.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["q"] = "star wars",
    ["sort_by_date"] = "0",
    ["type"] = "episode",
    ["offset"] = "0",
    ["len_min"] = "10",
    ["len_max"] = "30",
    ["genre_ids"] = "68,82",
    ["published_before"] = "1580172454000",
    ["published_after"] = "0",
    ["only_in"] = "title,description",
    ["language"] = "English",
    ["region"] = "",
    ["safe_mode"] = "0",
    ["unique_podcasts"] = "0",
    ["interviews_only"] = "0",
    ["sponsored_only"] = "0",
    ["page_size"] = "10",
};
var response = await client.Search(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-search)

### Typeahead

Typeahead search

`GET /typeahead`

Suggest search terms, podcast genres, and podcasts.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["q"] = "star wars",
    ["show_podcasts"] = "1",
    ["show_genres"] = "1",
    ["safe_mode"] = "0",
};
var response = await client.Typeahead(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-typeahead)

### SearchEpisodeTitles

Find individual episodes by searching for their titles

`GET /search_episode_titles`

Conduct targeted searches for individual episodes by title and refine results using the podcast id such as
Listen Notes Podcast ID, Apple Podcasts ID, Spotify ID, or RSS feed URL.
This endpoint is specially designed to streamline the import of specific episodes from platforms
like Apple Podcasts and Spotify into your application.
Compared to the GET /search endpoint, which performs full-text searches across multiple fields,
this endpoint focuses solely on episode titles for enhanced accuracy and performance.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["q"] = "Jerusalem Demsas on The Dispossessed",
};
var response = await client.SearchEpisodeTitles(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-search_episode_titles)

### SpellCheck

Spell check on a search term

`GET /spellcheck`

Suggest a list of words that correct the spelling errors of a search term. This endpoint is available only in the PRO/ENTERPRISE plan.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["q"] = "microsft stock",
};
var response = await client.SpellCheck(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-spellcheck)

### FetchRelatedSearches

Fetch related search terms

`GET /related_searches`

Suggest related search terms. The results are more comprehensive than from `GET /typeahead`. This endpoint is available only in the PRO/ENTERPRISE plan.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["q"] = "evergrande",
};
var response = await client.FetchRelatedSearches(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-related_searches)

### FetchTrendingSearches

Fetch trending search terms

`GET /trending_searches`

Fetch up to 10 most recent trending search terms on the Listen Notes platform.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
};
var response = await client.FetchTrendingSearches(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-trending_searches)

### FetchBestPodcasts

Fetch a list of best podcasts by genre

`GET /best_podcasts`

Get a list of curated best podcasts by genre,
which are curated by Listen Notes staffs based on various signals from the Internet, e.g.,
top charts on other podcast platforms, recommendations from mainstream media,
user activities on listennotes.com...
You can get the genre ids from `GET /genres` endpoint.
This endpoint returns same data as https://www.listennotes.com/best-podcasts/

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["genre_id"] = "93",
    ["page"] = "2",
    ["region"] = "us",
    ["sort"] = "listen_score",
    ["safe_mode"] = "0",
};
var response = await client.FetchBestPodcasts(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-best_podcasts)

### FetchPodcastById

Fetch detailed meta data and episodes for a podcast by id

`GET /podcasts/{id}`

Fetch detailed meta data and episodes for a specific podcast (up to 10 episodes each time).
You can use the **next_episode_pub_date** parameter to do pagination and fetch more episodes.
During pagination with **next_episode_pub_date**, an empty **episodes** array in the response signals that no more episodes are available.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["id"] = "4d3fe717742d4963a85562e9f84d8c79",
    ["next_episode_pub_date"] = "1479154463000",
    ["sort"] = "recent_first",
};
var response = await client.FetchPodcastById(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-podcasts-id)

### DeletePodcast

Request to delete a podcast

`DELETE /podcasts/{id}`

Podcast hosting services can use this endpoint to streamline the process of podcast deletion on behave of their users (podcasters). We will review the deletion request within 12 hours. If the podcast is already deleted, the "status" field in the response will be "deleted". Otherwise, the status field will be "in review". If you want to get a notification once the podcast is deleted, you can configure a webhook url in the dashboard: listennotes.com/api/dashboard/#webhooks

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["id"] = "4d3fe717742d4963a85562e9f84d8c79",
    ["reason"] = "the podcaster wants to delete it",
};
var response = await client.DeletePodcast(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#delete-api-v2-podcasts-id)

### FetchEpisodeById

Fetch detailed meta data for an episode by id

`GET /episodes/{id}`

Fetch detailed meta data for a specific episode.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["id"] = "6b6d65930c5a4f71b254465871fed370",
    ["show_transcript"] = "1",
};
var response = await client.FetchEpisodeById(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-episodes-id)

### BatchFetchEpisodes

Batch fetch basic meta data for episodes

`POST /episodes`

Batch fetch basic meta data for up to 10 episodes. This endpoint could be used to implement custom playlists for individual episodes. For detailed meta data of an individual episode, you need to use `GET /episodes/{id}`. This endpoint is available only in the PRO/ENTERPRISE plan.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["ids"] = "c577d55b2b2b483c969fae3ceb58e362,0f34a9099579490993eec9e8c8cebb82",
};
var response = await client.BatchFetchEpisodes(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#post-api-v2-episodes)

### BatchFetchPodcasts

Batch fetch basic meta data for podcasts

`POST /podcasts`

Batch fetch basic meta data for up to 10 podcasts.
This endpoint could be used to build something like OPML import,
allowing users to import a bunch of podcasts via rss urls.
For detailed meta data (including episodes) of an individual podcast, you need to use `GET /podcasts/{id}`. This endpoint is available only in the PRO/ENTERPRISE plan.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["ids"] = "3302bc71139541baa46ecb27dbf6071a,68faf62be97149c280ebcc25178aa731,37589a3e121e40debe4cef3d9638932a,9cf19c590ff0484d97b18b329fed0c6a",
    ["rsses"] = "https://rss.art19.com/recode-decode,https://rss.art19.com/the-daily,https://www.npr.org/rss/podcast.php?id=510331,https://www.npr.org/rss/podcast.php?id=510331",
    ["itunes_ids"] = "1457514703,1386234384,659155419",
    ["spotify_ids"] = "3DDfEsKDIDrTlnPOiG4ZF4,4qDNe5Gvl1XxdLinUGEXrC,23NZCM4ik6o3UYkM473Itz",
    ["show_latest_episodes"] = "1",
    ["next_episode_pub_date"] = "1557394247000",
};
var response = await client.BatchFetchPodcasts(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#post-api-v2-podcasts)

### FetchCuratedPodcastsListById

Fetch a curated list of podcasts by id

`GET /curated_podcasts/{id}`

Get detailed meta data of all podcasts in a specific curated list.
This endpoint returns same data as https://www.listennotes.com/curated-podcasts/

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["id"] = "SDFKduyJ47r",
};
var response = await client.FetchCuratedPodcastsListById(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-curated_podcasts-id)

### FetchPodcastGenres

Fetch a list of podcast genres

`GET /genres`

Get a list of podcast genres that are supported in Listen Notes.
The genre id can be passed to other endpoints as a parameter to get podcasts in a specific genre,
e.g., `GET /best_podcasts`, `GET /search`...
You may want to cache the list of genres on the client side.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["top_level_only"] = "1",
};
var response = await client.FetchPodcastGenres(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-genres)

### FetchPodcastRegions

Fetch a list of supported countries/regions for best podcasts

`GET /regions`

It returns a dictionary of country codes (e.g., us, gb...) & country names (United States, United Kingdom...). The country code is used in the query parameter **region** of `GET /best_podcasts`.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
};
var response = await client.FetchPodcastRegions(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-regions)

### FetchPodcastLanguages

Fetch a list of supported languages for podcasts

`GET /languages`

Get a list of languages that are supported in Listen Notes database. You can use the language string as query parameter in `GET /search`.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
};
var response = await client.FetchPodcastLanguages(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-languages)

### JustListen

Fetch a random podcast episode

`GET /just_listen`

Recently published episodes are more likely to be fetched. Good luck!

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
};
var response = await client.JustListen(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-just_listen)

### FetchCuratedPodcastsLists

Fetch curated lists of podcasts

`GET /curated_podcasts`

A bunch of curated lists from online media. For each list, you'll get basic info of up to 5 podcasts. To get detailed meta data of all podcasts in a specific list, you need to use `GET /curated_podcasts/{id}`. We add new curated lists to the database on a daily basis.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["page"] = "2",
};
var response = await client.FetchCuratedPodcastsLists(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-curated_podcasts)

### FetchRecommendationsForPodcast

Fetch recommendations for a podcast

`GET /podcasts/{id}/recommendations`

Fetch up to 8 podcast recommendations based on the given podcast id.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["id"] = "25212ac3c53240a880dd5032e547047b",
    ["safe_mode"] = "0",
};
var response = await client.FetchRecommendationsForPodcast(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-podcasts-id-recommendations)

### FetchRecommendationsForEpisode

Fetch recommendations for an episode

`GET /episodes/{id}/recommendations`

Fetch up to 8 episode recommendations based on the given episode id.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["id"] = "254444fa6cf64a43a95292a70eb6869b",
    ["safe_mode"] = "0",
};
var response = await client.FetchRecommendationsForEpisode(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-episodes-id-recommendations)

### SubmitPodcast

Submit a podcast to Listen Notes database

`POST /podcasts/submit`

Podcast hosting services can use this endpoint to help your users directly submit a new podcast to Listen Notes database. If the podcast doesn't exist in the database, "status" in the response will be "in review", and we'll review it within 12 hours. If the podcast exists, "status" in the response will be "found". If this submission is rejected, "status" in the response will be "rejected". You can use `POST /podcasts` to check if multiple podcasts exist in the database. If you want to get a notification once the podcast is accepted, you can either specify the "email" parameter or configure a webhook url in the dashboard: listennotes.com/api/dashboard/#webhooks

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["rss"] = "https://feeds.megaphone.fm/committed",
    ["email"] = "hello@example.com",
};
var response = await client.SubmitPodcast(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#post-api-v2-podcasts-submit)

### FetchPlaylistById

Fetch a playlist's info and items (i.e., episodes or podcasts).

`GET /playlists/{id}`

A playlist can contain both episodes and podcasts, shown in separate views,
just like playlists created via listennotes.com/listen/.
This endpoint fetches items from the saved default view unless **type** is specified.
The response type and listennotes_url describe the selected view.
You can use the **last_pub_date_ms** parameter to do pagination and fetch more items.
A playlist can be **public** (discoverable on ListenNotes.com),
**unlisted** (accessible to anyone who knows the playlist id),
or **private** (accessible when the API admin has active playlist membership).
Public and unlisted playlists can also be fetched by ID regardless of their owner.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["id"] = "m1pe7z60bsw",
    ["type"] = "episode_list",
    ["last_timestamp_ms"] = "0",
    ["sort"] = "recent_added_first",
};
var response = await client.FetchPlaylistById(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-playlists-id)

### FetchMyPlaylists

Fetch a list of your playlists.

`GET /playlists`

This endpoint lists playlists with an active membership for the API admin, including playlists they created or joined.
Each playlist includes its saved default **type** and a **listennotes_url** for that view.
You can use the **page** parameter to do pagination and fetch more playlists.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["sort"] = "recent_added_first",
    ["page"] = "1",
};
var response = await client.FetchMyPlaylists(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-playlists)

### FetchAudienceForPodcast

Fetch audience demographics for a podcast

`GET /podcasts/{id}/audience`

Fetch audience demographics for a podcast - 1) directly measured on the Listen Notes platform; 2) only supports audience breakdown by regions for now; 3) not every podcast has data.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["id"] = "25212ac3c53240a880dd5032e547047b",
};
var response = await client.FetchAudienceForPodcast(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-podcasts-id-audience)

### FetchPodcastsByDomain

Fetch podcasts by a publisher's domain name

`GET /podcasts/domains/{domain_name}`

Fetch podcasts by a publisher's domain name, e.g., nytimes.com, wondery.com, npr.org...
Each request will return up to 10 podcasts. You can use the `page` parameter to paginate.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["domain_name"] = "nytimes.com",
    ["page"] = "1",
};
var response = await client.FetchPodcastsByDomain(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#get-api-v2-podcasts-domains-domain_name)

### CreatePlaylist

Create a playlist.

`POST /playlists`

Create an empty playlist owned by the API admin. Name is required; description defaults to an empty string, visibility defaults to public, and type defaults to episode_list. Set type to podcast_list to make podcasts the default view. The response includes the saved type and its listennotes_url.

Only playlists owned by your admin API account can be modified; contributor membership does not grant write access.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["name"] = "My favorite podcasts",
    ["description"] = "Podcasts and episodes to revisit.",
    ["visibility"] = "public",
    ["type"] = "episode_list",
};
var response = await client.CreatePlaylist(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#post-api-v2-playlists)

### UpdatePlaylist

Update playlist metadata.

`PUT /playlists/{id}`

Update any subset of name, description, visibility, and type. Omitted fields remain unchanged; at least one field is required. Switching to private rotates the playlist RSS secret. Type selects the saved default view (episode_list or podcast_list) and the returned listennotes_url; changing it preserves all existing episodes and podcasts.

Only playlists owned by your admin API account can be modified; contributor membership does not grant write access.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["id"] = "m1pe7z60bsw",
    ["name"] = "My favorite podcasts",
    ["description"] = "Podcasts and episodes to revisit.",
    ["visibility"] = "public",
    ["type"] = "podcast_list",
};
var response = await client.UpdatePlaylist(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#put-api-v2-playlists-id)

### AddPlaylistItem

Add an episode or podcast to a playlist.

`POST /playlists/{id}/items`

Provide exactly one non-empty episode_id or podcast_id; an empty unused ID field is ignored. Invalid ID formats return 400 and identify the field. A missing episode or podcast returns 404 with an error such as "Episode not found: {episode_id}." or "Podcast not found: {podcast_id}.". Existing active items are reused (200); new or restored items return 201. Omitted notes preserve existing notes, including when restoring a deleted item; supplied notes replace them.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["id"] = "m1pe7z60bsw",
    ["episode_id"] = "e53e6992a5b7492f9ea6fcd85d9ad95f",
    ["notes"] = "Worth a listen.",
};
var response = await client.AddPlaylistItem(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#post-api-v2-playlists-id-items)

### DeletePlaylistItem

Remove an item from a playlist.

`DELETE /playlists/{id}/items/{item_id}`

Delete a playlist item. Repeating deletion of the same item succeeds. This does not delete the episode or podcast from the podcast database.

Only playlists owned by your admin API account can be modified; contributor membership does not grant write access.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["id"] = "m1pe7z60bsw",
    ["item_id"] = "23",
};
var response = await client.DeletePlaylistItem(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#delete-api-v2-playlists-id-items-item_id)

### UpdatePlaylistItemNotes

Update notes for a playlist item.

`PUT /playlists/{id}/items/{item_id}`

Replace item notes, or send an empty string to clear them. The item ID and added_at_ms remain unchanged.

Only playlists owned by your admin API account can be modified; contributor membership does not grant write access.

```csharp
using System;
using System.Collections.Generic;

using var client = new PodcastAPI.Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
var parameters = new Dictionary<string, string>
{
    ["id"] = "m1pe7z60bsw",
    ["item_id"] = "23",
    ["notes"] = "",
};
var response = await client.UpdatePlaylistItemNotes(parameters);
Console.WriteLine(response.ToJSON<dynamic>());
```

[Full API documentation](https://www.listennotes.com/api/docs/#put-api-v2-playlists-id-items-item_id)

<!-- END GENERATED API REFERENCE -->

## Development

From this repository with a .NET 8 or .NET 10 SDK installed:

```sh
dotnet restore src/PodcastAPI.net.sln --locked-mode
dotnet build src/PodcastAPI.net.sln --no-restore -c Release
dotnet test src/PodcastAPI.Tests --no-build -c Release --filter 'TestCategory!=Integration'
dotnet pack src/PodcastAPI --no-build -c Release -o artifacts
bash scripts/verify-package.sh
```

The tests target net8.0. With only a .NET 10 runtime installed, set
`DOTNET_ROLL_FORWARD=LatestMajor` when running tests to exercise that runtime.
Default tests intercept HTTP in memory; they never contact an API server.
The sample project compiles all generated README examples without executing them.
Dependency lockfiles are committed. To change dependencies, edit the project
files, run `dotnet restore src/PodcastAPI.net.sln --force-evaluate`, then review the locks.

Run the separately enabled integration suite with:

```sh
LISTEN_API_MOCK_INTEGRATION=1 dotnet test src/PodcastAPI.Tests --filter 'TestCategory=Integration'
```

It calls only `https://listen-api-test.listennotes.com/api/v2` with no API key,
including all five playlist writes. It never reads credentials from the environment
or permits a destination override. The mock does not prove persistence or production
permissions. CI runs these checks separately from offline tests.

API methods, contract fixtures, the example compilation file, test dispatch, and
marked README sections are generated from the Listen Notes monorepo's canonical
OpenAPI spec and .NET registry using `sync.py dotnet` inside its Vagrant environment.
Do not hand-edit generated files. The committed outputs let this repository build,
test, and package independently. Publishing to NuGet is a separate release step.
