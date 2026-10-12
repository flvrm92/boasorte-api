using System.Text.Json;

namespace BoaSorte.Domain.Tests.Draws;

public sealed record NormalizationCase(string Name, string[] Input, bool RemoveDuplicates, string[] Expected)
{
    public override string ToString() => Name;
}

/// <summary>
/// Cases shared with the client tests. Source of truth: boasorte-app/specs/001-sorteio-lista/contracts/normalization-cases.json.
/// </summary>
public static class NormalizationCases
{
    public const string FileName = "normalization-cases.json";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string LocalPath => Path.Combine(AppContext.BaseDirectory, "Fixtures", FileName);

    public static TheoryData<NormalizationCase> All()
    {
        using var stream = File.OpenRead(LocalPath);
        var document = JsonSerializer.Deserialize<CasesDocument>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"Could not read {FileName}.");

        return [.. document.Cases];
    }

    private sealed record CasesDocument(NormalizationCase[] Cases);
}
