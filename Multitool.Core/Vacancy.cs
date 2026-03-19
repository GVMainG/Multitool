using System.Text.Json;
using System.Text.Json.Serialization;

namespace Multitool.Core;

/// <summary>
/// Конвертер для дат в формате ISO 8601 с обработкой null
/// </summary>
public class IsoDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return DateTime.MinValue;

        if (reader.TryGetDateTime(out var dateTime))
            return dateTime;

        var str = reader.GetString();
        if (DateTime.TryParse(str, out dateTime))
            return dateTime;

        return DateTime.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("o"));
    }
}

/// <summary>
/// Модель вакансии hh.ru
/// </summary>
public class Vacancy
{
    /// <summary>
    /// ID вакансии
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Название вакансии
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Зарплата
    /// </summary>
    [JsonPropertyName("salary")]
    public Salary? Salary { get; set; }

    /// <summary>
    /// Город
    /// </summary>
    [JsonPropertyName("area")]
    public Area? Area { get; set; }

    /// <summary>
    /// Описание вакансии
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Требования
    /// </summary>
    [JsonPropertyName("key_skills")]
    public List<KeySkill>? KeySkills { get; set; }

    /// <summary>
    /// Условия работы
    /// </summary>
    [JsonPropertyName("work_schedule")]
    public WorkSchedule? WorkSchedule { get; set; }

    /// <summary>
    /// Тип занятости
    /// </summary>
    [JsonPropertyName("employment")]
    public Employment? Employment { get; set; }

    /// <summary>
    /// Работодатель
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer? Employer { get; set; }

    /// <summary>
    /// Дата публикации
    /// </summary>
    [JsonPropertyName("published_at")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime PublishedAt { get; set; }

    /// <summary>
    /// URL вакансии на hh.ru
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Опыт работы
    /// </summary>
    [JsonPropertyName("experience")]
    public Experience? Experience { get; set; }

    /// <summary>
    /// Образование
    /// </summary>
    [JsonPropertyName("education")]
    public Education? Education { get; set; }

    /// <summary>
    /// Языки
    /// </summary>
    [JsonPropertyName("languages")]
    public List<Language>? Languages { get; set; }

    /// <summary>
    /// Тестовые задания
    /// </summary>
    [JsonPropertyName("test_tasks")]
    public List<TestTask>? TestTasks { get; set; }

    /// <summary>
    /// Контакты
    /// </summary>
    [JsonPropertyName("contacts")]
    public Contacts? Contacts { get; set; }
}

/// <summary>
/// Зарплата
/// </summary>
public class Salary
{
    [JsonPropertyName("from")]
    public decimal? From { get; set; }

    [JsonPropertyName("to")]
    public decimal? To { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("gross")]
    public bool Gross { get; set; }
}

/// <summary>
/// Город
/// </summary>
public class Area
{
    [JsonPropertyName("id")]
    [JsonIgnore]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    [JsonIgnore]
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Навык
/// </summary>
public class KeySkill
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// График работы
/// </summary>
public class WorkSchedule
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Тип занятости
/// </summary>
public class Employment
{
    [JsonPropertyName("id")]
    [JsonIgnore]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Работодатель
/// </summary>
public class Employer
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("logo_urls")]
    [JsonIgnore]
    public LogoUrls? LogoUrls { get; set; }
}

/// <summary>
/// Логотипы работодателя
/// </summary>
public class LogoUrls
{
    [JsonPropertyName("90")]
    public string? Url90 { get; set; }

    [JsonPropertyName("240")]
    public string? Url240 { get; set; }

    [JsonPropertyName("original")]
    public string? Original { get; set; }
}

/// <summary>
/// Опыт работы
/// </summary>
public class Experience
{
    [JsonPropertyName("id")]
    [JsonIgnore]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Образование
/// </summary>
public class Education
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Язык
/// </summary>
public class Language
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Тестовое задание
/// </summary>
public class TestTask
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>
/// Контакты
/// </summary>
public class Contacts
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
/// Результат экспорта вакансий со статистикой
/// </summary>
public class VacancyExportResult
{
    /// <summary>
    /// Массив вакансий
    /// </summary>
    [JsonPropertyName("vacancies")]
    public List<Vacancy> Vacancies { get; set; } = new();

    /// <summary>
    /// Общая статистика
    /// </summary>
    [JsonPropertyName("statistics")]
    public ExportStatistics Statistics { get; set; } = new();
}

/// <summary>
/// Статистика экспорта
/// </summary>
public class ExportStatistics
{
    /// <summary>
    /// Общее количество вакансий
    /// </summary>
    [JsonPropertyName("total_vacancies")]
    public int TotalVacancies { get; set; }

    /// <summary>
    /// Топ-10 навыков
    /// </summary>
    [JsonPropertyName("top_skills")]
    public List<SkillStat> TopSkills { get; set; } = new();
}

/// <summary>
/// Статистика навыка
/// </summary>
public class SkillStat
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
