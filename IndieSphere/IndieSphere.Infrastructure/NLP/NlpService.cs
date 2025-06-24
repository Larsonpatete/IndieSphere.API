using Azure;
using Azure.AI.TextAnalytics;
using IndieSphere.Domain.Music;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace IndieSphere.Infrastructure.NLP;


public interface INlpService
{
    Task<MusicQuery> AnalyzeQueryAsync(string query);
}

public class NlpService : INlpService
{
    private readonly TextAnalyticsClient _azureClient;
    private readonly string? _projectName;
    private readonly string? _deploymentName;
    private readonly string? _deepSeekApiKey;
    private readonly string? _deepSeekUrl;
    private readonly HttpClient _httpClient;

    public NlpService(IConfiguration config, HttpClient httpClient)
    {
        var endpoint = config["AzureTextAnalytics:Endpoint"];
        var key = config["AzureTextAnalytics:Key"];
        _projectName = config["AzureTextAnalytics:ProjectName"];
        _deploymentName = config["AzureTextAnalytics:DeploymentName"];
        _azureClient = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(key));

        _deepSeekApiKey = config["DeepSeek:ApiKey"];
        _deepSeekUrl = config["DeepSeek:ApiUrl"];
        _httpClient = httpClient;
    }
    public async Task<MusicQuery> AnalyzeQueryAsync(string query)
    {
        var result = new MusicQuery();

        var encodedQuery = HttpUtility.UrlEncode(query);
        // Step 1: Classify intent
        result.Intent = await ClassifyIntentAsync(encodedQuery);

        // Step 2: Extract entities
        result.Entities = ExtractEntities(query);

        // Step 3: Enrich with Azure NER (optional)
        if (result.Entities.Count == 0)
        {
            await EnrichWithAzureNer(encodedQuery, result);
        }

        return result;
    }

    private async Task<SearchIntent> ClassifyIntentAsync(string query)
    {
        try
        {
            // Try Azure first
            var intentString = await GetDeepSeekIntent(query);
            return ParseIntent(intentString);
            //return await GetAzureIntent(query);
        }
        catch
        {
            // Fallback to DeepSeek custom classification
            //var intentString = await GetDeepSeekIntent(query);
            //return ParseIntent(intentString);
            return await GetAzureIntent(query);
        }
    }

    private async Task<string> GetDeepSeekIntent(string query)
    {
        //var requestBody = new
        //{
        //    model = "deepseek-chat",
        //    messages = new[]
        //    {
        //        new { role = "system", content = GetSystemPrompt() },
        //        new { role = "user", content = query }
        //    },
        //    temperature = 0.1,
        //    max_tokens = 20
        //};

        //var json = JsonConvert.SerializeObject(requestBody);
        //var response = await _httpClient.PostAsync(
        //    _deepSeekUrl,
        //    new StringContent(json, Encoding.UTF8, "application/json"));


        //var responseContent = await response.Content.ReadAsStringAsync();
        //Console.WriteLine($"Raw API Response: {responseContent}"); // Add this line

        //if (!response.IsSuccessStatusCode)
        //{
        //    throw new HttpRequestException($"API Error: {response.StatusCode} - {responseContent}");
        //}

        //var result = JsonConvert.DeserializeObject<DeepSeekResponse>(responseContent);
        //return result?.Choices?[0]?.Message?.Content?.Trim()?.ToLower() ?? "unknown";


        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_deepSeekApiKey}");

        var requestBody = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                    new
                    {
                        role = "system",
                        content = GetSystemPrompt()
                    },
                    new
                    {
                        role = "user",
                        content = query
                    }
                },
            temperature = 0.1,
            max_tokens = 20
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var response = await httpClient.PostAsync(
            _deepSeekUrl,
            new StringContent(json, Encoding.UTF8, "application/json"));

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<DeepSeekResponse>(responseContent);

        return result?.Choices?[0]?.Message?.Content?.Trim()?.ToLower() ?? "unknown";
    }

    private List<QueryEntity> ExtractEntities(string query)
    {
        var entities = new List<QueryEntity>();
        var lowerQuery = query.ToLower();

        // Artist detection
        var artistMatch = Regex.Match(lowerQuery, @"\b(by|artist|band)\s+([\w\s]+)\b");
        if (artistMatch.Success)
        {
            entities.Add(new QueryEntity(EntityType.Artist, artistMatch.Groups[2].Value.Trim()));
        }

        // Song detection
        var songMatch = Regex.Match(lowerQuery, @"\b(song|track|play)\s+['""]?([\w\s]+)['""]?\b");
        if (songMatch.Success)
        {
            entities.Add(new QueryEntity(EntityType.Song, songMatch.Groups[2].Value.Trim()));
        }

        // Album detection
        var albumMatch = Regex.Match(lowerQuery, @"\b(album|record)\s+['""]?([\w\s]+)['""]?\b");
        if (albumMatch.Success)
        {
            entities.Add(new QueryEntity(EntityType.Album, albumMatch.Groups[2].Value.Trim()));
        }

        // Genre detection
        var genreMatch = Regex.Match(lowerQuery, @"\b(genre|like)\s+(\w+)\b");
        if (genreMatch.Success)
        {
            entities.Add(new QueryEntity(EntityType.Genre, genreMatch.Groups[2].Value.Trim()));
        }

        return entities;
    }

    private async Task EnrichWithAzureNer(string query, MusicQuery domain)
    {
        try
        {
            var response = await _azureClient.RecognizeEntitiesAsync(query);
            foreach (var entity in response.Value)
            {
                var entityType = MapAzureEntityType(entity.Category);
                if (entityType != EntityType.Unknown)
                {
                    domain.Entities.Add(new QueryEntity(entityType, entity.Text));
                }
            }
        }
        catch
        {
            // Azure NER failed, use fallback
            AddFallbackEntity(query, domain);
        }
    }

    private void AddFallbackEntity(string query, MusicQuery domain)
    {
        // Simple heuristic: Last phrase is likely the entity
        var lastPhrase = query.Split(' ').Last();
        if (!string.IsNullOrWhiteSpace(lastPhrase))
        {
            var entityType = domain.Intent switch
            {
                SearchIntent.ArtistSearch => EntityType.Artist,
                SearchIntent.SongSearch => EntityType.Song,
                SearchIntent.AlbumSearch => EntityType.Album,
                SearchIntent.GenreSearch => EntityType.Genre,
                _ => EntityType.Unknown
            };

            if (entityType != EntityType.Unknown)
            {
                domain.Entities.Add(new QueryEntity(entityType, lastPhrase));
            }
        }
    }
    private SearchIntent ParseIntent(string intentString) => intentString switch
    {
        "similarsongsearch" or "similar_song" => SearchIntent.SimilarSongSearch,
        "artistsearch" or "artist" => SearchIntent.ArtistSearch,
        "genresearch" or "genre" => SearchIntent.GenreSearch,
        "albumsearch" or "album" => SearchIntent.AlbumSearch,
        "songsearch" or "song" => SearchIntent.SongSearch,
        "similarartistsearch" or "similar_artist" => SearchIntent.SimilarArtistSearch,
        _ => SearchIntent.Unknown
    };

    private SearchIntent MapAzureCategory(string category) => category.ToLower() switch
    {
        "similarsong" => SearchIntent.SimilarSongSearch,
        "artistsearch" => SearchIntent.ArtistSearch,
        "genresearch" => SearchIntent.GenreSearch,
        "albumsearch" => SearchIntent.AlbumSearch,
        "songsearch" => SearchIntent.SongSearch,
        "similarartist" => SearchIntent.SimilarArtistSearch,
        _ => SearchIntent.Unknown
    };

    private EntityType MapAzureEntityType(EntityCategory category)
    {
        // Convert enum to string for switch expression
        return category.ToString() switch
        {
            "Person" => EntityType.Artist,
            "Event" => EntityType.Song,
            "Product" => EntityType.Album,
            "Organization" => EntityType.Artist,
            _ => EntityType.Unknown
        };
    }

    private string GetSystemPrompt() => """
                **Role**: You are an intent classification system for music queries.  
        **Task**: Classify user queries into one of these intents using EXACTLY these pattern rules:

        ### Classification Rules
        1. `songSearch` (must include "song" + title):  
           - `"song [song_title]"`  
           - `"song [song_title] by [artist]"`  
           *(e.g., "song Creep", "song Karma Police by Radiohead")*

        2. `similarSongSearch` (must include "like"/"similar to"):  
           - `"song like [song_title]"`  
           - `"song similar to [song_title]"`  
           - `"song like [song_title] by [artist]"`  
           - `"song similar to [song_title] by [artist]"`  
           *(e.g., "song like Creep", "song similar to Karma Police by Radiohead")*

        3. `artistSearch` (must start with artist/band/singer):  
           - `"artist [artist_name]"`  
           - `"band [artist_name]"`  
           - `"singer [artist_name]"`  
           *(e.g., "artist Radiohead", "band Nirvana")*

        4. `similarArtistSearch` (must include "like"/"similar to"):  
           - `"artist like [artist_name]"`  
           - `"band like [artist_name]"`  
           - `"singer like [artist_name]"`  
           - `"artist similar to [artist_name]"`  
           - `"band similar to [artist_name]"`  
           - `"singer similar to [artist_name]"`  
           *(e.g., "artist like Radiohead", "band similar to Nirvana")*

        5. `genreSearch` (must include "genre"):  
           - `"genre [genre_name]"`  
           - `"[genre_name] genre"`  
           *(e.g., "genre rock", "alternative genre")*

        6. `albumSearch` (must include "album"):  
           - `"album [album_title]"`  
           - `"album [album_title] by [artist]"`  
           *(e.g., "album OK Computer", "album Nevermind by Nirvana")*

        ### Critical Constraints
        - **Keyword Requirements**:  
          - `songSearch` MUST include "song"  
          - `similarSongSearch` MUST include "like" or "similar to" AFTER "song"  
          - `albumSearch` MUST include "album"  
          - `genreSearch` MUST include "genre" at start/end  
        - **Title Conflicts**:  
          - If song title contains "like" (e.g., "I Like That"):  
            → `"song I Like That"` = `songSearch`  
            → `"song like I Like That"` = `similarSongSearch`  
        - **No Variations**:  
          - Do NOT accept synonyms (only "song", not "track"/"tune")  
          - Do NOT accept partial matches (must match patterns EXACTLY)  
        **Output Format**:
        - Always return intent names in lowercase
        - Use ONLY these exact intent names:
          "songsearch"
          "similarsongsearch"
          "artistsearch"
          "similarartistsearch"
          "genresearch"
          "albumsearch"
        """;

        //@"
        //Classify music queries into ONE intent:
        //- similarSongSearch: Finds similar songs (e.g., 'songs like Creep')
        //- artistSearch: Finds artists (e.g., 'show Radiohead')
        //- genresearch: Finds by genre (e.g., 'play jazz')
        //- albumSearch: Finds albums (e.g., 'album OK Computer')
        //- songSearch: Finds specific songs (e.g., 'play Bohemian Rhapsody')
        //- similarArtistSearch: Finds similar artists (e.g., 'artists like Coldplay')

    //Respond ONLY with the intent name. Examples:
    //User: 'tracks like Yesterday' → similarSongSearch
    //User: 'hip hop music' → genresearch";


    private async Task<SearchIntent> GetAzureIntent(string query)
    {
        List<string> batchedDocuments = new()
        {
            query
        };

        ClassifyDocumentOperation operation = await _azureClient.SingleLabelClassifyAsync(WaitUntil.Completed, batchedDocuments, _projectName, _deploymentName);

        await foreach (ClassifyDocumentResultCollection documentsInPage in operation.Value)
        {
            foreach (ClassifyDocumentResult documentResult in documentsInPage)
            {
                if (documentResult.HasError)
                {
                    throw new InvalidOperationException($"Document error code: {documentResult.Error.ErrorCode}, Message: {documentResult.Error.Message}");
                }

                var bestCategory = documentResult.ClassificationCategories
                    .OrderByDescending(c => c.ConfidenceScore)
                    .FirstOrDefault();

                if (!EqualityComparer<ClassificationCategory>.Default.Equals(bestCategory, default))
                {
                    return MapAzureCategory(bestCategory.Category);
                }
            }
        }


        throw new InvalidOperationException("No classification result found.");
    }

}
public record NlpResult(string queryType, decimal confidence);


public class DeepSeekResponse
{
    public Choice[] Choices { get; set; }
}

public class Choice
{
    public Message Message { get; set; }
}

public class Message
{
    public string Content { get; set; }
}





//public async Task<string> DeepSeekIntentClassifier(string query)
//{
//    using var httpClient = new HttpClient();
//    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {DeepSeekApiKey}");

//    var requestBody = new
//    {
//        model = "deepseek-chat",
//        messages = new[]
//        {
//            new
//            {
//                role = "system",
//                content = GetSystemPrompt()
//            },
//            new
//            {
//                role = "user",
//                content = query
//            }
//        },
//        temperature = 0.1,
//        max_tokens = 20
//    };

//    var json = JsonConvert.SerializeObject(requestBody);
//    var response = await httpClient.PostAsync(
//        DeepSeekURL,
//        new StringContent(json, Encoding.UTF8, "application/json"));

//    var responseContent = await response.Content.ReadAsStringAsync();
//    var result = JsonConvert.DeserializeObject<DeepSeekResponse>(responseContent);

//    return result?.Choices?[0]?.Message?.Content?.Trim()?.ToLower() ?? "unknown";
//}


//private SearchIntent ParseIntent(string intentString)
//{
//    return intentString switch
//    {
//        "similarsongsearch" or "similar_song" => SearchIntent.SimilarSongSearch,
//        "artistsearch" or "artist" => SearchIntent.ArtistSearch,
//        "genresearch" or "genre" => SearchIntent.GenreSearch,
//        "albumsearch" or "album" => SearchIntent.AlbumSearch,
//        "songsearch" or "song" => SearchIntent.SongSearch,
//        "similarartistsearch" or "similar_artist" => SearchIntent.SimilarArtistSearch,
//        _ => SearchIntent.Unknown
//    };
//}

//private string GetSystemPrompt() => @"
//    Classify music queries into ONE intent:
//    - similarSongSearch: Finds similar songs (e.g., 'songs like Creep')
//    - artistSearch: Finds artists (e.g., 'show Radiohead')
//    - genresearch: Finds by genre (e.g., 'play jazz')
//    - albumSearch: Finds albums (e.g., 'album OK Computer')
//    - songSearch: Finds specific songs (e.g., 'play Bohemian Rhapsody')
//    - similarArtistSearch: Finds similar artists (e.g., 'artists like Coldplay')

//    Respond ONLY with the intent name. Examples:
//    User: 'tracks like Yesterday' → similarSongSearch
//    User: 'hip hop music' → genresearch";
