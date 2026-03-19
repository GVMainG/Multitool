using System.Text.Json;
using HhVacancyExporter.Models;

namespace HhVacancyExporter.Services;

public class VacancyProcessor
{
    private readonly HhApiClient _client;

    // cache by id -> ResultItem (for reuse)
    private readonly Dictionary<string, ResultItem> _cache = new(StringComparer.Ordinal);

    public VacancyProcessor(HhApiClient client)
    {
        _client = client;
    }

    public async Task<List<ResultItem>> ProcessAsync(List<ParsedLink> parsedLinks, CancellationToken ct)
    {
        var results = new List<ResultItem>();

        // keep first representative source_url per id
        var byId = new Dictionary<string, ParsedLink>(StringComparer.Ordinal);

        foreach (var p in parsedLinks)
        {
            if (!p.IsValidDomain)
            {
                results.Add(new ResultItem
                {
                    SourceUrl = p.SourceUrl,
                    Status = "error",
                    Vacancy = null,
                    Error = new ErrorOut { Code = p.ErrorCode ?? "Invalid", Message = p.ErrorMessage ?? "" }
                });
                continue;
            }

            if (!p.HasId || string.IsNullOrEmpty(p.VacancyId))
            {
                results.Add(new ResultItem
                {
                    SourceUrl = p.SourceUrl,
                    Status = "error",
                    Vacancy = null,
                    Error = new ErrorOut { Code = p.ErrorCode ?? "InvalidId", Message = p.ErrorMessage ?? "" }
                });
                continue;
            }

            if (!byId.ContainsKey(p.VacancyId))
            {
                byId[p.VacancyId] = p;
            }
        }

        foreach (var kv in byId)
        {
            if (ct.IsCancellationRequested) break;

            var id = kv.Key;
            var representative = kv.Value.SourceUrl;

            if (_cache.TryGetValue(id, out var cached))
            {
                // cached exists, but we must preserve the source_url of representative
                var item = cached with { SourceUrl = representative };
                results.Add(item);
                continue;
            }

            // per-vacancy timeout 5s
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var (dto, httpStatus, errorCode, errorMessage) = await _client.GetVacancyAsync(id, cts.Token);

            ResultItem ri;
            if (dto != null)
            {
                ri = new ResultItem
                {
                    SourceUrl = representative,
                    Status = "ok",
                    Vacancy = MapVacancy(dto),
                    Error = null
                };
            }
            else
            {
                ri = new ResultItem
                {
                    SourceUrl = representative,
                    Status = "error",
                    Vacancy = null,
                    Error = new ErrorOut { Code = errorCode ?? "HttpError", Message = errorMessage ?? "", HttpStatus = httpStatus }
                };
            }

            _cache[id] = ri;
            results.Add(ri);
        }

        return results;
    }

    private VacancyOut MapVacancy(ApiVacancyDto dto)
    {
        var v = new VacancyOut
        {
            Id = dto.Id,
            Url = dto.AlternateUrl ?? dto.Url ?? "",
            Name = dto.Name,
            Description = dto.Description,
            Salary = dto.Salary == null ? null : new SalaryOut { From = dto.Salary.From, To = dto.Salary.To, Currency = dto.Salary.Currency, Gross = dto.Salary.Gross },
            KeySkills = dto.KeySkills?.Select(k => k.Name).ToList() ?? new List<string>(),
            Employer = dto.Employer == null ? null : new EmployerOut { Id = dto.Employer.Id, Name = dto.Employer.Name, Url = dto.Employer.AlternateUrl },
            Area = dto.Area == null ? null : new AreaOut { Id = dto.Area.Id, Name = dto.Area.Name },
            PublishedAt = dto.PublishedAt
        };
        return v;
    }
}

public record ResultItem
{
    public string SourceUrl { get; init; } = "";
    public string Status { get; init; } = "";
    public VacancyOut? Vacancy { get; init; }
    public ErrorOut? Error { get; init; }
}
