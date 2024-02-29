# Allegro.pl REST API

[![License](https://img.shields.io/github/license/JadeX/AllegroAPI)](https://github.com/JadeX/Allegro/blob/master/LICENSE.txt)
[![NuGet Downloads](https://img.shields.io/nuget/dt/JadeX.Allegro.svg)](https://www.nuget.org/packages/JadeX.AllegroAPI/)
[![NuGet](https://img.shields.io/nuget/vpre/JadeX.Allegro.svg)](https://www.nuget.org/packages/JadeX.AllegroAPI/)
![.NET Standard](https://img.shields.io/badge/NETStandard-2.0/2.1-blue.svg)
![.NET 6-8](https://img.shields.io/badge/NET-6.0--8.0-purple.svg)
[![build](https://github.com/JadeX/Allegro/actions/workflows/build.yml/badge.svg)](https://github.com/JadeX/Allegro/actions/workflows/build.yml)

Unofficial implementation of Allegro.pl commerce platform REST API for .NET as documented on
[https://developer.allegro.pl](https://developer.allegro.pl)

> [!WARNING] 
> ## Current state: ALPHA
> All endpoints are code-generated providing entirety of the API for consumption. Please be advised `IAllegroApi.generated.cs` may change dramatically between regenerations requiring substantial code changes for calling API endpoints. Needs better deterministic name generation handling.

### Highlights

| Function                         | Description  |
| -------------------------------- | -----:|
| Strongly-typed API communication | With [Refit REST library](https://github.com/reactiveui/refit), communication with Allegro API is conducted in a type-safe manner, no magic strings or studying documentation necessary, everything is in the IntelliSense. |
| Code generated OpenAPI endpoints | All endpoints are code generated from most current OpenAPI specification published by Allegro.pl using [Refitter](https://github.com/christianhelle/refitter), therefore is always up-to-date, error-prone and includes full IntelliSense. |
| Authentication API               | When instance is provided with valid credentials & refresh token, library will ensure all requests have valid authentication headers. Tokens are validated and automatically refreshed only as needed. <br> *Only [device flow](https://developer.allegro.pl/tutorials/uwierzytelnianie-i-autoryzacja-zlq9e75GdIR#device-flow) is currently supported.* |

## Install with NuGet

```sh
dotnet add package JadeX.AllegroAPI --prerelease
```

## Basic Usage (Console)
> [!IMPORTANT]
> Make sure you have registered your application with [Allegro API Apps (Sandbox)](https://apps.developer.allegro.pl.allegrosandbox.pl) or [Allegro API Apps (Production)](https://apps.developer.allegro.pl), for this you need active account on respective environment.
> Take note of `CLIENT ID` and `CLIENT SECRET`.

```csharp
using JadeX.AllegroAPI;

var allegroRestClient = new AllegroRestClient(x =>
{
    x.Environment = AllegroEnvironment.Sandbox;
    x.Authentication.Flow = AuthenticationFlow.Device;
    x.Authentication.ClientId = "{YOUR CLIENT ID}";
    x.Authentication.ClientSecret = "{YOUR CLIENT SECRET}";
});

var deviceCodeResponse = await allegroRestClient.AuthenticationService.GetDeviceCode().ConfigureAwait(false); // Don't call this very often
Console.WriteLine($"Open {deviceCodeResponse.VerificationUrlComplete} in browser and follow instructions there ... Once finished, press enter to obtain access token and call some API endpoint.");
Console.ReadLine();
await allegroRestClient.AuthenticationService.GetAccessTokens().ConfigureAwait(false); // Valid for 12 hours, need to call RefreshTokens() afterwards

// Now it's possible to call API enpoints

var offers = await allegroRestClient.API.SearchOffersUsingGET(limit: 1).ConfigureAwait(false);
Console.Write($"You have total of {offers.TotalCount} offers.");
if (offers.TotalCount > 0)
{
    Console.WriteLine($" First one is called '{offers.Offers?.First().Name}'.");
}

var orderEvents = await allegroRestClient.API.GetOrderEventsUsingGET().ConfigureAwait(false);
Console.WriteLine($"There is total of {orderEvents.Events.Count} order events:");
Console.WriteLine("Types are: " + string.Join(", ", orderEvents.Events.Select(x => x.Type.ToString())));
Console.ReadLine();
```
> [!CAUTION]
> You can't get access tokens with `GetAccessTokens()` until device code is validated with browser under desired user account (link in VerificationUrlComplete)!

### Advanced Authentication

Clearly previous example is handling authentication very poorly, calling `GetDeviceCode()` every time instance is created is not just incredibly cumbersome, but deemed unwated by the API and will very soon hit rate limiter like requiring solving CAPTCHA. To solve this `AccessToken` and `RefreshToken` need to be reused.

> [!TIP]
> `AllegroRestClient` remembers most recent device code and access/refresh tokens internally, you should deal with them only during initialization.

```csharp
using JadeX.AllegroAPI;

var AllegroRestClient = new AllegroRestClient(x =>
{
    x.Environment = AllegroEnvironment.Sandbox;
    x.Authentication.Flow = AuthenticationFlow.Device;
    x.Authentication.ClientId = "{YOUR CLIENT ID}";
    x.Authentication.ClientSecret = "{YOUR CLIENT SECRET}";

    x.Authentication.RefreshToken = "{retrieve your RefreshToken from secure persistent storage}";
    // While not absolutely necessary you can also store/retrieve AccessToken, prevents unnecessary token refresh (especially useful in testing).
    x.Authentication.AccessToken = "{retrieve your AccessToken from secure persistent storage}";

    x.Authentication.OnAccessTokenChanged += OnAccessTokenChanged;
    x.Authentication.OnRefreshTokenChanged += OnRefreshTokenChanged;
});

static void OnAccessTokenChanged(object? sender, string? accessToken)
{
    // Store new accessToken in secure persistent storage like database or user-secrets file
}

static void OnRefreshTokenChanged(object? sender, string? refreshToken)
{
    // Store new refreshToken in secure persistent storage like database or user-secrets file
}
```

## Versioning Scheme

This library is using [SemVer 2.0.0](https://semver.org/) as it's versioning scheme.
