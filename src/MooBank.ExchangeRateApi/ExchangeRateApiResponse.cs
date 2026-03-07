using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Asm.MooBank.ExchangeRateApi;

internal record ExchangeRateApiResponse
{
    public required string Result { get; init; }
    public required string Documentation { get; init; }

    [JsonPropertyName("terms_of_use")]
    public required string TermsOfUse { get; init; }

    [JsonPropertyName("time_last_update_unix")]
    public required long TimeLastUpdateUnix { get; init; }

    [JsonPropertyName("time_last_update_utc")]
    [JsonConverter(typeof(DateTimeConverter))]
    public required DateTime TimeLastUpdateUtc { get; init; }

    [JsonPropertyName("time_next_update_unix")]
    public required long TimeNextUpdateUnix { get; init; }

    [JsonPropertyName("time_next_update_utc")]
    [JsonConverter(typeof(DateTimeConverter))]
    public required DateTime TimeNextUpdateUtc { get; init; }

    [JsonPropertyName("base_code")]
    public required string BaseCode
    {
        get; init;
    }

    [JsonPropertyName("conversion_rates")]
    public Dictionary<string, decimal> ConversionRates { get; init; } = [];
}


public class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateTime.ParseExact(reader.GetString()!, "ddd, dd MMM yyyy HH:mm:ss +0000", CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
