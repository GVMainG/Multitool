using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Multitool.Core;

namespace Multitool.Infrastructure;

/// <summary>
/// Сервис для работы с API hh.ru
/// </summary>
public class HHService : IHHService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public HHService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.hh.ru/")
        };

        // User-Agent обязателен для API hh.ru в формате: Приложение/Версия (+URL)
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Multitool/1.0 (+https://github.com/Multitool)");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    public async Task<VacancyResult> GetVacancyAsync(int vacancyId, CancellationToken ct = default)
    {
        var url = $"vacancies/{vacancyId}";
        
        int attempts = 0;
        int maxAttempts = 3;
        int delayMs = 200;

        while (true)
        {
            attempts++;
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                using var resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    var vacancy = await resp.Content.ReadFromJsonAsync<Vacancy>(_jsonOptions, ct);
                    
                    if (vacancy == null)
                        return VacancyResult.Fail("Пустой ответ от API", vacancyId);
                    
                    return VacancyResult.Ok(vacancy, vacancyId);
                }
                else if (resp.StatusCode == HttpStatusCode.NotFound)
                {
                    return VacancyResult.Fail($"Вакансия с ID {vacancyId} не найдена", vacancyId);
                }
                else if (resp.StatusCode == (HttpStatusCode)429 || ((int)resp.StatusCode >= 500 && (int)resp.StatusCode < 600))
                {
                    if (attempts >= maxAttempts)
                        return VacancyResult.Fail($"Ошибка API: HTTP {(int)resp.StatusCode}", vacancyId);
                    
                    await Task.Delay(delayMs, ct);
                    delayMs *= 2;
                    continue;
                }
                else
                {
                    return VacancyResult.Fail($"Ошибка API: HTTP {(int)resp.StatusCode}", vacancyId);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return VacancyResult.Fail("Операция отменена по таймауту", vacancyId);
            }
            catch (JsonException ex)
            {
                return VacancyResult.Fail($"Ошибка парсинга JSON: {ex.Message}", vacancyId);
            }
            catch (Exception ex)
            {
                if (attempts >= maxAttempts)
                    return VacancyResult.Fail($"Ошибка: {ex.Message}", vacancyId);
                
                try { await Task.Delay(delayMs, ct); } catch { }
                delayMs *= 2;
            }
        }
    }

    public async Task<IReadOnlyList<VacancyResult>> GetVacanciesAsync(IEnumerable<int> vacancyIds, CancellationToken ct = default)
    {
        var results = new List<VacancyResult>();

        foreach (var id in vacancyIds)
        {
            var result = await GetVacancyAsync(id, ct);
            results.Add(result);

            // Небольшая задержка между запросами для защиты от rate limiting
            if (!ct.IsCancellationRequested)
                await Task.Delay(100, ct);
        }

        return results.AsReadOnly();
    }
}
