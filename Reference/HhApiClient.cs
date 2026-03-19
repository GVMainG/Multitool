using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Http;
using System.Net.Http.Headers;
using HhVacancyExporter.Models;

namespace HhVacancyExporter.Services;

public class HhApiClient
{
    public const string UserAgent = "HhVacancyExporter/1.0 (+https://example.com)";
    private readonly IHttpClientFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public HhApiClient(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<(ApiVacancyDto? dto, int? httpStatus, string? errorCode, string? errorMessage)> GetVacancyAsync(string id, CancellationToken ct)
    {
        var client = _factory.CreateClient("hh");
        var url = $"https://api.hh.ru/vacancies/{id}";

        int attempts = 0;
        int maxAttempts = 3;
        int delayMs = 200;

        while (true)
        {
            attempts++;
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.UserAgent.ParseAdd(UserAgent);
                using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    var stream = await resp.Content.ReadAsStreamAsync(ct);
                    var dto = await JsonSerializer.DeserializeAsync<ApiVacancyDto>(stream, _jsonOptions, ct);
                    return (dto, (int)resp.StatusCode, null, null);
                }
                else if (resp.StatusCode == HttpStatusCode.NotFound)
                {
                    return (null, (int)resp.StatusCode, "NotFound", "Vacancy not found");
                }
                else if (resp.StatusCode == (HttpStatusCode)429 || ((int)resp.StatusCode >= 500 && (int)resp.StatusCode < 600))
                {
                    if (attempts >= maxAttempts) return (null, (int)resp.StatusCode, "HttpError", $"HTTP {(int)resp.StatusCode}" );
                    await Task.Delay(delayMs, ct);
                    delayMs *= 2;
                    continue;
                }
                else
                {
                    return (null, (int)resp.StatusCode, "HttpError", $"HTTP {(int)resp.StatusCode}");
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return (null, null, "Timeout", "Operation canceled/timeout");
            }
            catch (Exception ex)
            {
                if (attempts >= maxAttempts) return (null, null, "HttpError", ex.Message);
                try { await Task.Delay(delayMs, ct); } catch { }
                delayMs *= 2;
            }
        }
    }
}
