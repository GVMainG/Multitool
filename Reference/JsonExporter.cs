using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using HhVacancyExporter.Models;

namespace HhVacancyExporter.Services;

public class JsonExporter
{
    // Use UnsafeRelaxedJsonEscaping so non-ASCII (Cyrillic) is written as readable characters
    private readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task ExportAsync(List<ResultItem> items, string outputPath, CancellationToken ct)
    {
        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        await using var fs = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(fs, items, _opts, ct);
    }
}
