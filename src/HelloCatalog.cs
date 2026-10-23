// Hello catalog plugin: answers every search with one greeting track, album and artist.

using System.Text.Json;
using HostConfig = CatalogProviderWorld.wit.imports.hearthsound.plugin.v0_3_0.HostConfigInterop;
using Types = CatalogProviderWorld.wit.imports.hearthsound.plugin.v0_3_0.ITypes;

namespace CatalogProviderWorld.wit.exports.hearthsound.plugin.v0_3_0;

/// Constants and record builders shared by the catalog and lifecycle implementations.
internal static class Hello
{
    internal const string DefaultGreeting = "Hello";
    internal const string TrackId = "hello-track-1";
    internal const string AlbumId = "hello-album-1";
    internal const string ArtistId = "hello-artist-1";
    internal const string AlbumTitle = "Hello Album";
    internal const string ArtistName = "Hello Artist";

    /// Extracts the greeting from a server configuration document.
    ///
    /// A missing `greeting` property yields the default. A document that is not a JSON
    /// object, or a `greeting` that is not a string, is rejected.
    internal static string GreetingFrom(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("server config must be a JSON object");
        }
        if (!root.TryGetProperty("greeting", out JsonElement greeting))
        {
            return DefaultGreeting;
        }
        if (greeting.ValueKind != JsonValueKind.String)
        {
            throw new FormatException("greeting must be a string");
        }
        return greeting.GetString()!;
    }

    /// Reads the greeting from this instance's server configuration.
    ///
    /// Throws a provider error carrying `invalid-config` when the configuration cannot be parsed.
    internal static string ConfiguredGreeting()
    {
        try
        {
            return GreetingFrom(HostConfig.ServerConfig());
        }
        catch (Exception e) when (e is JsonException or FormatException)
        {
            throw new WitException(Types.ProviderError.InvalidConfig(e.Message), 0);
        }
    }

    /// Builds the provider error that reports an unknown identifier.
    internal static WitException NotFound() =>
        new(Types.ProviderError.NotFound(), 0);

    /// Returns whether a search with the given kind filter asks for `wanted`.
    ///
    /// An empty filter means every kind was asked for.
    internal static bool Wants(List<Types.EntityKind> kinds, Types.EntityKind wanted) =>
        kinds.Count == 0 || kinds.Contains(wanted);

    /// Clamps an unsigned paging value to the range accepted by LINQ paging operators.
    internal static int Clamp(uint value) => (int)Math.Min(value, (uint)int.MaxValue);

    internal static List<Types.ArtistCredit> HelloCredits() =>
        new() { new Types.ArtistCredit(ArtistName, new List<Types.IdPair>()) };

    internal static Types.AlbumRef HelloAlbumRef() =>
        new(AlbumId, AlbumTitle, HelloCredits(), (ushort)2026, new List<Types.Image>());

    internal static Types.Artist HelloArtist() =>
        new(ArtistId, ArtistName, new List<Types.Image>(), new List<Types.IdPair>());

    internal static Types.Track HelloTrack(string greeting, string text) =>
        new(
            TrackId,
            $"{greeting}, {text}",
            HelloCredits(),
            HelloAlbumRef(),
            180000u,
            (ushort)1,
            (ushort)1,
            null,
            false,
            new List<Types.IdPair> { new("hello", TrackId) });

    internal static Types.Album HelloAlbum(string greeting) =>
        new(
            AlbumId,
            AlbumTitle,
            HelloCredits(),
            "2026-01-01",
            "album",
            null,
            new List<string>(),
            new List<Types.Image>(),
            new List<Types.Track> { HelloTrack(greeting, "world") },
            new List<Types.IdPair>());
}

/// Implements the catalog interface.
public class CatalogImpl : ICatalog
{
    public static ICatalog.SearchResults Search(ICatalog.SearchQuery query)
    {
        if (query.limit == 0 || query.offset > 0)
        {
            return new ICatalog.SearchResults(
                new List<Types.Track>(),
                new List<Types.AlbumRef>(),
                new List<Types.Artist>());
        }

        string greeting = Hello.ConfiguredGreeting();
        return new ICatalog.SearchResults(
            Hello.Wants(query.kinds, Types.EntityKind.TRACK)
                ? new List<Types.Track> { Hello.HelloTrack(greeting, query.text) }
                : new List<Types.Track>(),
            Hello.Wants(query.kinds, Types.EntityKind.ALBUM)
                ? new List<Types.AlbumRef> { Hello.HelloAlbumRef() }
                : new List<Types.AlbumRef>(),
            Hello.Wants(query.kinds, Types.EntityKind.ARTIST)
                ? new List<Types.Artist> { Hello.HelloArtist() }
                : new List<Types.Artist>());
    }

    public static Types.Track GetTrack(string id)
    {
        if (id != Hello.TrackId)
        {
            throw Hello.NotFound();
        }
        return Hello.HelloTrack(Hello.ConfiguredGreeting(), "world");
    }

    public static Types.Album GetAlbum(string id)
    {
        if (id != Hello.AlbumId)
        {
            throw Hello.NotFound();
        }
        return Hello.HelloAlbum(Hello.ConfiguredGreeting());
    }

    public static Types.Artist GetArtist(string id)
    {
        if (id != Hello.ArtistId)
        {
            throw Hello.NotFound();
        }
        return Hello.HelloArtist();
    }

    public static List<Types.AlbumRef> ArtistAlbums(string id, uint limit, uint offset)
    {
        if (id != Hello.ArtistId)
        {
            throw Hello.NotFound();
        }
        return new List<Types.AlbumRef> { Hello.HelloAlbumRef() }
            .Skip(Hello.Clamp(offset))
            .Take(Hello.Clamp(limit))
            .ToList();
    }

    public static List<Types.Track> ArtistTopTracks(string id, uint limit)
    {
        if (id != Hello.ArtistId)
        {
            throw Hello.NotFound();
        }
        return new List<Types.Track> { Hello.HelloTrack(Hello.ConfiguredGreeting(), "world") }
            .Take(Hello.Clamp(limit))
            .ToList();
    }

    public static List<Types.Track> LookupIsrc(string isrc) => new();
}

/// Implements the lifecycle interface.
public class LifecycleImpl : ILifecycle
{
    public static void ValidateConfig(string serverConfig)
    {
        try
        {
            Hello.GreetingFrom(serverConfig);
        }
        catch (Exception e) when (e is JsonException or FormatException)
        {
            throw new WitException(new List<string> { e.Message }, 0);
        }
    }

    public static string Health() => "hello-catalog ready";
}
