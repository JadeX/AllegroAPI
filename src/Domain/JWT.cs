namespace JadeX.AllegroAPI.Domain;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using global::JWT;

public class JWT
{
    [JsonPropertyName("user_name")]
    public string? Username { get; set; }

    [JsonPropertyName("scope")]
    public List<string>? Scope { get; set; }

    [JsonPropertyName("allegro_api")]
    public bool AllegroApi { get; set; }

    [JsonPropertyName("iss")]
    public string? Iss { get; set; }

    [JsonPropertyName("exp")]
    public int Exp { get; set; }

    [JsonPropertyName("jti")]
    public string? Jti { get; set; }

    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }

    public bool Active => this.Exp > UnixEpoch.GetSecondsSince(DateTime.UtcNow);
}
