namespace BoaSorte.Domain.Tests.Draws;

/// <summary>Detects drift between the local copy of the shared cases and the spec, when both repos are checked out side by side.</summary>
public class NormalizationCasesSyncTests
{
    [Fact]
    public void Local_copy_matches_the_spec()
    {
        var specPath = FindSpecCopy();
        if (specPath is null)
        {
            Assert.Skip("boasorte-app repository not found next to this one.");
        }

        Assert.Equal(Normalize(File.ReadAllText(specPath)), Normalize(File.ReadAllText(NormalizationCases.LocalPath)));
    }

    private static string? FindSpecCopy()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "BoaSorte.slnx")))
            {
                var candidate = Path.Combine(
                    dir.FullName, "..", "boasorte-app", "specs", "001-sorteio-lista", "contracts", NormalizationCases.FileName);
                return File.Exists(candidate) ? candidate : null;
            }
        }

        return null;
    }

    private static string Normalize(string text) => text.ReplaceLineEndings("\n").Trim();
}
