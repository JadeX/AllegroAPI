namespace JadeX.AllegroAPI.Domain;

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DebugHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid().ToString();
        var msg = $"[{id} - Request]";

        Debug.WriteLine($"{msg}========Start==========");
        Debug.WriteLine($"{msg} {request.Method} {request.RequestUri?.PathAndQuery} {request.RequestUri?.Scheme}/{request.Version}");
        Debug.WriteLine($"{msg} Host: {request.RequestUri?.Scheme}://{request.RequestUri?.Host}");

        foreach (var header in request.Headers)
        {
            Debug.WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                Debug.WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");
            }

#if NETSTANDARD
            var result = await request.Content.ReadAsStringAsync();
#else
                var result = await request.Content.ReadAsStringAsync(cancellationToken);
#endif

            Debug.WriteLine($"{msg} Content:");
            Debug.WriteLine($"{msg} {string.Join("", result)}");
        }

        var start = DateTime.Now;

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var end = DateTime.Now;

        Debug.WriteLine($"{msg} Duration: {end - start}");
        Debug.WriteLine($"{msg}==========End==========");

        msg = $"[{id} - Response]";
        Debug.WriteLine($"{msg}=========Start=========");

        var resp = response;

        Debug.WriteLine($"{msg} {request.RequestUri?.Scheme.ToUpper(CultureInfo.InvariantCulture)}/{resp.Version} {(int)resp.StatusCode} {resp.ReasonPhrase}");

        foreach (var header in resp.Headers)
        {
            Debug.WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");
        }

        if (resp.Content != null)
        {
            foreach (var header in resp.Content.Headers)
            {
                Debug.WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");
            }

            if (resp.Content is StringContent || this.IsTextBasedContentType(resp.Headers) || this.IsTextBasedContentType(resp.Content.Headers))
            {
                start = DateTime.Now;
#if NETSTANDARD
                var result = await resp.Content.ReadAsStringAsync();
#else
                var result = await resp.Content.ReadAsStringAsync(cancellationToken);
#endif
                end = DateTime.Now;

                Debug.WriteLine($"{msg} Content:");
                Debug.WriteLine($"{msg} {string.Join("", result)}");
                Debug.WriteLine($"{msg} Duration: {end - start}");
            }
        }

        Debug.WriteLine($"{msg}==========End==========");
        return response;
    }

    private readonly string[] types = ["html", "text", "xml", "json", "txt", "x-www-form-urlencoded"];

    private bool IsTextBasedContentType(HttpHeaders headers)
    {
        if (!headers.TryGetValues("Content-Type", out var values))
        {
            return false;
        }

        var header = string.Join(" ", values).ToLowerInvariant();

        return this.types.Any(header.Contains);
    }
}
