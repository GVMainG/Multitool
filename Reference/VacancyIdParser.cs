using System.Text.RegularExpressions;
namespace HhVacancyExporter.Services;

public record ParsedLink(string SourceUrl, bool IsValidDomain, bool HasId, string? VacancyId, string? ErrorCode, string? ErrorMessage);

public class VacancyIdParser
{
    private static readonly Regex VacancyRegex = new(@"/vacancy/(?<id>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IdOnlyRegex = new(@"^\d+$", RegexOptions.Compiled);

    public ParsedLink Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new ParsedLink(input ?? string.Empty, false, false, null, "InvalidUrl", "Url is empty");

        var trimmed = input.Trim();

        // If input is purely numeric, treat as vacancy id
        if (IdOnlyRegex.IsMatch(trimmed))
        {
            var vid = trimmed;
            var canonical = $"https://hh.ru/vacancy/{vid}";
            return new ParsedLink(canonical, true, true, vid, null, null);
        }

        // Try parse as uri
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            // try with scheme
            if (Uri.TryCreate("https://" + trimmed, UriKind.Absolute, out uri))
            {
                // continue
            }
            else
            {
                return new ParsedLink(input, false, false, null, "InvalidUrl", "Url is not valid");
            }
        }

        var host = uri.Host; // e.g. hh.ru or m.hh.ru
        if (!host.EndsWith("hh.ru", StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedLink(input, false, false, null, "InvalidDomain", "Domain is not hh.ru");
        }

        var m = VacancyRegex.Match(uri.AbsolutePath);
        if (!m.Success)
        {
            return new ParsedLink(input, true, false, null, "InvalidId", "Could not extract vacancy id from path");
        }

        var id = m.Groups["id"].Value;
        return new ParsedLink(input, true, true, id, null, null);
    }
}
