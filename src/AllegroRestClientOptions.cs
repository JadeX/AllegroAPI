namespace JadeX.AllegroAPI;
using System;

public delegate string OnDeviceCodeChanged();
public delegate string OnRefreshTokenChanged();
public delegate string OnAccessTokenChanged();

public class AllegroRestClientOptions
{
    public AllegroEnvironment Environment { get; set; } = AllegroEnvironment.Sandbox;
    public AllegroAuthentication Authentication { get; set; } = new();
    public AllegroInternals Allegro { get; set; } = new();
    public string EnvironmentApiUrl =>
        this.Environment == AllegroEnvironment.Sandbox ? this.Allegro.SandboxApiHost : this.Allegro.ProductionApiHost;
    public string EnvironmentAuthUrl =>
        this.Environment == AllegroEnvironment.Sandbox ? this.Allegro.SandboxAuthHost : this.Allegro.ProductionAuthHost;
    public TimeSpan Timeout { get; set; }
}

public class AllegroInternals
{
    public string Language { get; set; } = "en-US";
    public string SandboxApiHost { get; set; } = "api.allegro.pl.allegrosandbox.pl";
    public string SandboxAuthHost { get; set; } = "allegro.pl.allegrosandbox.pl";
    public string ProductionApiHost { get; set; } = "api.allegro.pl";
    public string ProductionAuthHost { get; set; } = "allegro.pl";
    public int ClientIdLength { get; set; } = 32;
    public int ClientSecretLength { get; set; } = 64;
    public int DeviceCodeLength { get; set; } = 32;
    public int AccessTokenLength { get; set; } = 1764;
    public int RefreshTokenLength { get; set; } = 1824;
}

public class AllegroAuthentication
{
    private string? deviceCode;
    private string? refreshToken;
    private string? accessToken;
    public AuthenticationFlow Flow { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? DeviceCode { get => this.deviceCode; set { this.deviceCode = value; OnDeviceCodeChanged?.Invoke(this, value); } }
    public string? AccessToken { get => this.accessToken; set { this.accessToken = value; OnAccessTokenChanged?.Invoke(this, value); } }
    public string? RefreshToken { get => this.refreshToken; set { this.refreshToken = value; OnRefreshTokenChanged?.Invoke(this, value); } }
    public event EventHandler<string?>? OnDeviceCodeChanged;
    public event EventHandler<string?>? OnAccessTokenChanged;
    public event EventHandler<string?>? OnRefreshTokenChanged;
}

public enum AllegroEnvironment
{
    Sandbox,
    Production
}

public enum AuthenticationFlow
{
    Device
}
