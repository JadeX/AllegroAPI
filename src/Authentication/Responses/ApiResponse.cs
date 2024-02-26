namespace JadeX.AllegroAPI;

using System.Text.Json.Serialization;

public record ApiResponse
{
    public bool IsSuccess => string.IsNullOrEmpty(this.Error);

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
}
