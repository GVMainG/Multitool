namespace Multitool.Core;

/// <summary>
/// Результат загрузки вакансии
/// </summary>
public class VacancyResult
{
    public bool Success { get; set; }
    public Vacancy? Vacancy { get; set; }
    public string? Error { get; set; }
    public int VacancyId { get; set; }

    public static VacancyResult Ok(Vacancy vacancy, int id) => new()
    {
        Success = true,
        Vacancy = vacancy,
        VacancyId = id
    };

    public static VacancyResult Fail(string error, int id) => new()
    {
        Success = false,
        Error = error,
        VacancyId = id
    };
}

/// <summary>
/// Интерфейс сервиса для работы с API hh.ru
/// </summary>
public interface IHHService
{
    /// <summary>
    /// Получение вакансии по ID
    /// </summary>
    Task<VacancyResult> GetVacancyAsync(int vacancyId, CancellationToken ct = default);

    /// <summary>
    /// Получение нескольких вакансий по ID
    /// </summary>
    Task<IReadOnlyList<VacancyResult>> GetVacanciesAsync(IEnumerable<int> vacancyIds, CancellationToken ct = default);
}
