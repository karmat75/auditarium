// SPDX-License-Identifier: MIT
using System.Text.RegularExpressions;
using Xunit;

namespace Auditarium.Infrastructure.Security.Tests;

public sealed partial class DocumentationQualityTests
{
    [Fact]
    public void Maintained_markdown_links_resolve_to_repository_paths()
    {
        var root = FindRepositoryRoot();
        var brokenLinks = MaintainedMarkdownFiles(root)
            .SelectMany(path => ExtractMarkdownDestinations(File.ReadAllText(path))
                .Where(IsRepositoryLocal)
                .Where(destination => !TargetExists(root, path, destination))
                .Select(destination => $"{Path.GetRelativePath(root, path)}: {destination}"))
            .ToArray();

        Assert.True(brokenLinks.Length == 0,
            "Broken repository-local Markdown links:" + Environment.NewLine + string.Join(Environment.NewLine, brokenLinks));
    }

    [Fact]
    public void Adr_files_and_index_are_consistent()
    {
        var root = FindRepositoryRoot();
        var adrDirectory = Path.Combine(root, "documents", "ADR");
        var indexPath = Path.Combine(adrDirectory, "README.md");
        Assert.True(File.Exists(indexPath), "documents/ADR/README.md must exist.");

        var adrFiles = Directory.EnumerateFiles(adrDirectory, "*.md")
            .Where(path => !string.Equals(Path.GetFileName(path), "README.md", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        Assert.All(adrFiles, path => Assert.Matches(AdrFileNamePattern(), Path.GetFileName(path)));

        var numberedAdrs = adrFiles
            .Select(path => new AdrFile(path, int.Parse(Path.GetFileName(path)[..4])))
            .OrderBy(adr => adr.Number)
            .ToArray();
        Assert.Equal(numberedAdrs.Select(adr => adr.Number).Distinct().Count(), numberedAdrs.Length);
        Assert.Equal(Enumerable.Range(1, numberedAdrs.Length), numberedAdrs.Select(adr => adr.Number));

        foreach (var adr in numberedAdrs)
        {
            var content = File.ReadAllText(adr.Path);
            var title = AdrTitlePattern().Match(content);
            Assert.True(title.Success, $"{Path.GetFileName(adr.Path)} must start with '# ADR NNNN – ...'.");
            Assert.Equal(adr.Number.ToString("0000"), title.Groups["number"].Value);
            foreach (var section in new[] { "Status", "Context", "Decision", "Consequences", "References" })
                Assert.Matches($@"(?m)^## {section}\s*$", content);

            var status = StatusSectionPattern().Match(content);
            Assert.True(status.Success && !string.IsNullOrWhiteSpace(status.Groups["value"].Value),
                $"{Path.GetFileName(adr.Path)} must have a non-empty Status section.");
        }

        var indexedFileNames = ExtractMarkdownDestinations(File.ReadAllText(indexPath))
            .Select(LocalPathPart)
            .Select(Path.GetFileName)
            .Where(name => name is not null && AdrFileNamePattern().IsMatch(name))
            .Select(name => name!)
            .ToArray();
        var numberedFileNames = numberedAdrs.Select(adr => Path.GetFileName(adr.Path)).Order().ToArray();
        Assert.Equal(indexedFileNames.Length, indexedFileNames.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(numberedFileNames, indexedFileNames.Order(StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void Current_workflow_guidance_does_not_contain_stale_active_workflow_claims()
    {
        var root = FindRepositoryRoot();
        var currentGuidance = new[]
        {
            "README.md",
            "CONTRIBUTING.md",
            "documents/README.md",
            "documents/Prompts/IssueImplementation_Template.md"
        };
        var staleClaims = new[]
        {
            "Soll-/Pflichtenheft ist die normative Quelle",
            "Bearbeite ausschließlich Work Package",
            "selected from chapter 20",
            "aus Kapitel 20 ausgewählt",
            "Work Packages as the active backlog"
        };

        foreach (var relativePath in currentGuidance)
        {
            var content = File.ReadAllText(Path.Combine(root, relativePath));
            Assert.DoesNotContain(staleClaims, claim => content.Contains(claim, StringComparison.OrdinalIgnoreCase));
        }

        Assert.False(File.Exists(Path.Combine(root, "documents", "Prompts", "NewWorkPackage_Template.md")));
    }

    [Fact]
    public void Current_documentation_entry_points_reference_current_sources()
    {
        var root = FindRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        var documentsReadme = File.ReadAllText(Path.Combine(root, "documents", "README.md"));

        Assert.Contains("(documents/Architecture/Overview.md)", readme, StringComparison.Ordinal);
        Assert.Contains("historische fachliche und technische Provenienz", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("(Architecture/Overview.md)", documentsReadme, StringComparison.Ordinal);
        Assert.Contains("Auditarium_Soll_Pflichtenheft.md", documentsReadme, StringComparison.Ordinal);
        Assert.Contains("Eingefrorenes historisches Material", documentsReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("GitHub Issues", documentsReadme, StringComparison.Ordinal);
        Assert.Contains("Aktiver Backlog", documentsReadme, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> MaintainedMarkdownFiles(string root) =>
        new[] { Path.Combine(root, "README.md"), Path.Combine(root, "CONTRIBUTING.md") }
            .Concat(Directory.EnumerateFiles(Path.Combine(root, "documents"), "*.md", SearchOption.AllDirectories));

    private static IEnumerable<string> ExtractMarkdownDestinations(string content) =>
        MarkdownLinkPattern().Matches(content).Select(match => match.Groups["destination"].Value.Trim());

    private static bool IsRepositoryLocal(string destination)
    {
        var path = LocalPathPart(destination);
        return !string.IsNullOrWhiteSpace(path)
            && !path.StartsWith('#')
            && !path.StartsWith("//", StringComparison.Ordinal)
            && !Uri.TryCreate(path, UriKind.Absolute, out _);
    }

    private static bool TargetExists(string root, string sourcePath, string destination)
    {
        var path = Uri.UnescapeDataString(LocalPathPart(destination)).Replace('/', Path.DirectorySeparatorChar);
        var target = path.StartsWith(Path.DirectorySeparatorChar)
            ? Path.Combine(root, path.TrimStart(Path.DirectorySeparatorChar))
            : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourcePath)!, path));
        return IsWithinRepository(root, target) && (File.Exists(target) || Directory.Exists(target));
    }

    private static bool IsWithinRepository(string root, string path) =>
        path.StartsWith(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);

    private static string LocalPathPart(string destination)
    {
        var path = destination.Trim().Trim('<', '>');
        var fragmentIndex = path.IndexOf('#');
        return fragmentIndex >= 0 ? path[..fragmentIndex] : path;
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Auditarium.sln"))) return directory.FullName;
        throw new DirectoryNotFoundException("Could not locate the Auditarium repository root.");
    }

    private sealed record AdrFile(string Path, int Number);

    // This deliberately covers inline links and images only; reference-style links and complex escaped destinations need a Markdown parser.
    [GeneratedRegex("!?(?:\\[[^\\]]*\\])\\((?<destination><[^>]+>|[^)\\s]+)(?:\\s+\\\"[^\\\"]*\\\")?\\)")]
    private static partial Regex MarkdownLinkPattern();

    [GeneratedRegex("^\\d{4}-[a-z0-9]+(?:-[a-z0-9]+)*\\.md$")]
    private static partial Regex AdrFileNamePattern();

    [GeneratedRegex("(?m)^# ADR (?<number>\\d{4}) [–-] .+$")]
    private static partial Regex AdrTitlePattern();

    [GeneratedRegex("(?ms)^## Status\\s*$\\r?\\n(?<value>.*?)(?=^## |\\z)")]
    private static partial Regex StatusSectionPattern();
}
