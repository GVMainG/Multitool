using System.Text;
namespace HhVacancyExporter.Services;

public class LinkSourceReader
{
    public Task<List<string>> ReadAsync(List<string> argsUrls, string? inputPath, CancellationToken ct)
    {
        var result = new List<string>(argsUrls ?? new());
        if (!string.IsNullOrEmpty(inputPath) && File.Exists(inputPath))
        {
            var lines = File.ReadAllLines(inputPath, Encoding.UTF8).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim());
            result.AddRange(lines);
        }

        return Task.FromResult(result);
    }
}
